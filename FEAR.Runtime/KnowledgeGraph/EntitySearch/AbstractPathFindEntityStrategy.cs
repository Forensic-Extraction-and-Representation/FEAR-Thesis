using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphDB.Cypher;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using FEAR.Domain.KnowledgeGraph.TypeService;
using FEAR.Runtime.KnowledgeGraph.EntitySearch.InMemory;
using VDS.RDF;
using VDS.RDF.Query;

namespace FEAR.Runtime.KnowledgeGraph.EntitySearch
{
    public abstract class AbstractPathFindEntityStrategy<T> : IFindEntityStrategy
    {
        protected ITypeConversionService TypeConversionService { get; }
        protected IRemoteGraph<T> CompleteGraph { get; }
        public AbstractPathFindEntityStrategy(IRealGraph completeGraph, ITypeConversionService typeConversionService)
        {
            if (completeGraph is IRemoteGraph<T>)
                CompleteGraph = (IRemoteGraph<T>)completeGraph;
            else
                throw new Exception("Could not create Remote Strategy for non IRemoteGraph graph type");

            TypeConversionService = typeConversionService;
        }

        public abstract BaseConstructQueryGenerator<T> CreateQueryGenerator(IRealGraph completeGraph, ITypeConversionService typeConversionService);

        public KGResponse<Entity> FindEntity(GraphUpdateContext findEntityContext)
        {
            var leafs = findEntityContext.EntityTreeNode.LeafEntities;
            var flattenedTree = findEntityContext.EntityTreeNode.IdentifiedEntities;
            var maxDepth = flattenedTree.Values.Max(t => t.Depth);

            IMaterializedGraph tempGraph = findEntityContext.TemporaryGraph;

            BaseConstructQueryGenerator<T> cqg = CreateQueryGenerator(CompleteGraph, TypeConversionService);
            T spq1 = cqg.ConstructLiteralEntityLoadQuery(leafs.Values);
            CompleteGraph.ExecuteConstructQuery(spq1, tempGraph);

            // Once loaded, we need to link entities in the graph to their graph reference
            cqg.LoadLeafUriFromGraph(leafs.Values, tempGraph);

            // We also need to apply these over the flattenedTree
            foreach (var x in flattenedTree)
            {
                foreach (var n in x.Value.Entity.IdentifyingNodes)
                {
                    if (leafs.ContainsKey(n.Hash))
                    {
                        n.EntityGraphReference = leafs[n.Hash].EntityGraphReference;
                    }
                }
            }

            // Will need to load collections
            var collections = findEntityContext.EntityTreeNode.Collections;

            T spq2 = cqg.ConstructNodeEntityLoadQuery(flattenedTree.ToDictionary(k => k.Value, v => v.Value.Depth), collections, maxDepth);
            if (spq2 != null)
                CompleteGraph.ExecuteConstructQuery(spq2, tempGraph);

            foreach (GraphUpdateCollection col in collections.Where(t => t.Collection.CollectionType == "rdf:List"))
            {
                if (col.ParentReference.ParentEntity.EntityGraphReference?.EntityGraphUri == null)
                {
                    DefaultFindEntityStrategy dfes = new DefaultFindEntityStrategy(tempGraph, TypeConversionService);
                    var findResult = dfes.FindEntity(new GraphUpdateContext(tempGraph, findEntityContext.GraphManager, findEntityContext.GraphCodifyService, col.ParentReference.ParentEntity.Entity));
                    if (findResult.IsSuccess)
                    {
                        col.ParentReference.ParentEntity.EntityGraphReference = new GraphReference() { EntityGraphUri = (findResult.Value.Individual.Resource as IUriNode).Uri };
                    }
                }

                if (col.ParentReference.ParentEntity.EntityGraphReference?.EntityGraphUri != null)
                {
                    var spq3 = cqg.LoadListCollections(col);
                    if (spq3 != null)
                    {
                        var colGraph = findEntityContext.GraphManager.CreateTemporaryGraph("CollectionGraph", "Temporary graph for collection load");
                        CompleteGraph.ExecuteConstructQuery(spq3, colGraph);

                        /*
                         * THIS NEED TO BE SPECIFIC TO THE TYPE OF DATABASE WE ARE USING
                         * CYPHER GRAPHS WILL LOAD AS ARRAYS OR MULTIPLE RELATIONSHIPS 
                         * AND GENERIC COLLECTIONS, WITH NO CONCEPT OF FIRST/REST, 
                         * OR RDF:_1, 2, ETC.
                         */
                        var rdfFirstNode = tempGraph.GetNodeForUri("rdf:first");
                        var rdfRestNode = tempGraph.GetNodeForUri("rdf:rest");
                        var rdfNilNode = tempGraph.GetNodeForUri("rdf:nil");

                        var parentEntityUri = (IUriNode)tempGraph.GetNodeForUri(col.ParentReference.ParentEntity.EntityGraphReference.EntityGraphUri);
                        var parentEntityNode = tempGraph.GetIndividual(parentEntityUri);

                        var parentEntityPropertyUri = (IUriNode)tempGraph.GetNodeForUri(col.ParentReference.PropertyName);
                        var parentEntityPropertyNode = tempGraph.GetIndividual(parentEntityPropertyUri);

                        var currentNode = parentEntityNode.Value.TriplesWithSubject.WithPredicate(parentEntityPropertyUri).FirstOrDefault(t => t.Object.NodeType == NodeType.Blank)?.Object as IBlankNode;
                        if (currentNode == null)
                        {
                            currentNode = tempGraph.CreateBlankNode();
                            tempGraph.Graph.Assert(parentEntityUri, parentEntityPropertyUri, currentNode);
                        }

                        for (int i = 0; i < colGraph.Graph.Triples.Count; i++)
                        {
                            var triple = colGraph.Graph.Triples.ElementAt(i);
                            tempGraph.Graph.Assert(new Triple(currentNode, rdfFirstNode, triple.Object));

                            if (i < colGraph.Graph.Triples.Count - 1)
                            {
                                var blankNode = tempGraph.Graph.CreateBlankNode();
                                tempGraph.Graph.Assert(new Triple(currentNode, rdfRestNode, blankNode));
                                currentNode = blankNode;
                            }
                            else
                            {
                                tempGraph.Graph.Assert(new Triple(currentNode, rdfRestNode, rdfNilNode));
                            }
                        }
                    }
                }
            }

            // Will need to reformat rdf:List collections as these will all be set to 'rdf:first'

            return tempGraph.FindEntity(findEntityContext);
        }
    }
}