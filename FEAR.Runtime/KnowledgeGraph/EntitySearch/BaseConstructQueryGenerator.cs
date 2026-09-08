using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphManager;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using FEAR.Domain.KnowledgeGraph.TypeService;
using VDS.RDF;

namespace FEAR.Runtime.KnowledgeGraph.EntitySearch
{
    public abstract class BaseConstructQueryGenerator<T> : IConstructQueryGenerator<T>
    {
        /// <summary>
        /// Constructs a query to load entities that are only identified by literal values.
        /// </summary>
        /// <param name="leafs"></param>
        /// <returns></returns>
        public abstract T ConstructLiteralEntityLoadQuery(ICollection<GraphUpdateEntity> leafs);
        public abstract T ConstructNodeEntityLoadQuery(Dictionary<IdentifiedEntity, int> flattenedTree, IList<GraphUpdateCollection> collections, int maxDepth);
        public abstract T LoadListCollections(GraphUpdateCollection collection);

        public BaseConstructQueryGenerator(IRemoteGraph<T> graph, ITypeConversionService typeConversionService)
        {
            Graph = graph;
            TypeConversionService = typeConversionService;
        }

        protected IRemoteGraph<T> Graph { get; }
        protected IGraphManager GraphManager => Graph.GraphManager;
        protected ITypeConversionService TypeConversionService { get; }
        protected string TypePredicate => $"{QueryParameterPrefix}TypePredicate";
        protected INode TypeUriNode => Graph.GetNodeForUri("rdf:type");
         
        protected string RdfAPredicate => $"{QueryParameterPrefix}RdfAPredicate";
        protected INode RdfAUriNode => Graph.GetNodeForUri("rdf:a");
         
        protected string RdfFirstPredicate => $"{QueryParameterPrefix}RdfFirstPredicate";
        protected INode RdfFirstUriNode => Graph.GetNodeForUri("rdf:first");
         
        protected string RdfRestPredicate => $"{QueryParameterPrefix}RdfRestPredicate";
        protected INode RdfRestUriNode => Graph.GetNodeForUri("rdf:rest");

        protected abstract string QueryParameterPrefix { get; }
        protected abstract string QueryStatementTripleTerminator { get; }

        public Uri UriFor(string uri)
        {
            return Graph.GraphManager.GetUriFromPrefixNotation(uri).Value;
        }

        public IUriNode UriNodeFor(string uri)
        {
            return Graph.GetNodeForUri(UriFor(uri)) as IUriNode;
        }

        public IUriNode UriNodeFor(Uri uri)
        {
            return Graph.GetNodeForUri(uri) as IUriNode;
        }

        protected virtual ILiteralNode LiteralNodeFor(object objectValue, string objectTypeUri)
        {
            Uri typeUri = Graph.GraphManager.GetUriFromPrefixNotation(objectTypeUri).Value;
            GraphManager.OntologyGraph.NamespaceMap.ReduceToQName(objectTypeUri, out string qName);
            string objString = TypeConversionService.ConvertToString(objectValue, qName, false);

            // handle as a literal
            return Graph.GetOrCreateLiteralNode(objString, typeUri.ToString()).Value;
        }

        /// <summary>
        /// Find all leaf entities in the graph and loads their URIs for use in loading the entity referenced nodes.
        /// </summary>
        /// <param name="leafs">The leaf entities that were queried for loading</param>
        /// <param name="tempGraph">The graph that contains the loaded entities that existed</param>
        public virtual void LoadLeafUriFromGraph(ICollection<GraphUpdateEntity> leafs, IMaterializedGraph tempGraph)
        {
            foreach (var leaf in leafs)
            {
                var ent = tempGraph.FindEntity(new GraphUpdateContext(tempGraph, GraphManager, null, leaf.Entity));
                if (ent.IsSuccess)
                {
                    leaf.EntityGraphReference = new GraphReference() { EntityGraphUri = (ent.Value.Individual.Resource as IUriNode)?.Uri };
                }
            }
        }
    }
}