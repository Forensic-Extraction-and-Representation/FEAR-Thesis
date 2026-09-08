using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.Collections;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using VDS.RDF;

namespace FEAR.Runtime.KnowledgeGraph.Cypher.Collections
{
    /// <summary>
    /// Implements a strategy for creating and updating RDF collections (Bag and List) in a knowledge graph.
    /// This class provides logic to materialize collection properties and update the working graph
    /// according to the RDF specification for collections.
    /// </summary>
    public class CollectionCreationStrategy : BaseCollectionOperationStrategy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionCreationStrategy"/> class.
        /// </summary>
        /// <param name="guc">The graph update context for the operation.</param>
        public CollectionCreationStrategy(GraphUpdateContext guc) : base(guc) { }

        /// <summary>
        /// Creates the base triples for a collection, including the blank node for the list head
        /// and the triple linking the target entity to the collection node.
        /// </summary>
        /// <param name="cosc">The operation context containing collection and graph information.</param>
        /// <param name="triples">The set of triples to be asserted as part of the operation.</param>
        private void CreateBaseTriples(CollectionOperationStrategyContext cosc, TriplesSet triples)
        {
            if (cosc.ListNode == null)
            {
                cosc.ListNode = WorkingGraph.CreateBlankNode();

                //triples.Assert.Add(new Triple(cosc.ListNode, RdfAUriNode, cosc.Graph.Graph.CreateUriNode(cosc.CollectionProperty.CollectionType)));
                
                // Ensures that the list is added to the target entity
                triples.Assert.Add(new Triple(cosc.TargetEntity.Individual.Resource, cosc.PropertyUriNode, cosc.ListNode));
            }
        }

        /// <summary>
        /// Executes a Bag operation on the collection using the provided context.
        /// Adds or updates the Bag collection in the working graph, ensuring each element is represented as an rdf:_n property.
        /// </summary>
        /// <param name="cosc">The operation context containing collection and graph information.</param>
        /// <returns>Result of the collection operation, including triples to assert.</returns>
        public override CollectionOperationResult BagOperation(CollectionOperationStrategyContext cosc)
        {
            CollectionOperationResult cor = new CollectionOperationResult();
            CreateBaseTriples(cosc, cor.TripleSet);

            for (int i = 0; i < cosc.CollectionProperty.Elements.Count; i++)
            {
                // Get the element from the collection property
                var element = cosc.CollectionProperty.Elements[i];
                // Get the target node for the element (ensures it's updated or created in the working graph)
                INode targetNode = TargetNodeForElement(element);

                // See if there is a triple for the list node and the entity, don't care about the "number"
                var triplesWithSubjectObject = WorkingGraph.Graph.GetTriplesWithSubjectObject(cosc.ListNode, targetNode);
                if (!triplesWithSubjectObject.Any())
                {
                    var collectionTriples = WorkingGraph.Graph.GetTriplesWithSubject(cosc.ListNode).ToList();
                    collectionTriples.AddRange(cor.TripleSet.Assert.Where(t => t.Subject == cosc.ListNode));
                    int counter = 1;
                    for (; counter <= collectionTriples.Count() + 10; counter++)
                    {
                        var counterUri = WorkingGraph.Graph.CreateUriNode($"rdf:_{counter}");
                        var currentTriple = collectionTriples.FirstOrDefault(t => (t.Predicate as IUriNode).Uri.ToString() == counterUri.ToString());
                        if (currentTriple == null)
                        {
                            break;
                        }
                    }

                    cor.TripleSet.Assert.Add(new Triple(cosc.ListNode, WorkingGraph.Graph.CreateUriNode($"rdf:_{counter}"), targetNode));
                }
            }

            return cor;
        }

        /// <summary>
        /// Converts a collection element to the appropriate RDF node (entity, URI, or literal).
        /// </summary>
        /// <param name="element">The element to convert.</param>
        /// <returns>The corresponding RDF node.</returns>
        private INode TargetNodeForElement(object element)
        {
            INode targetNode = null;
            if (element is Entity)
            {
                GraphUpdateContext guc = GraphUpdateContext.DuplicateContextForEntity(element as Entity);
                var listTargetEntity = WorkingGraph.CreateOrUpdateEntity(guc);
                targetNode = listTargetEntity.Individual.Resource;
            }
            else
            {
                if (element is Uri)
                {
                    targetNode = WorkingGraph.Graph.CreateUriNode(element as Uri);
                }
                else
                {
                    // This is a literal, so we need to add it to the list of nodes to add.
                    targetNode = WorkingGraph.Graph.CreateLiteralNode(element.ToString());
                }
            }

            return targetNode;
        }

        /// <summary>
        /// Executes a List operation on the collection using the provided context.
        /// Adds or updates the List collection in the working graph, constructing the linked list structure as per RDF List semantics.
        /// </summary>
        /// <param name="cosc">The operation context containing collection and graph information.</param>
        /// <returns>Result of the collection operation, including triples to assert.</returns>
        public override CollectionOperationResult ListOperation(CollectionOperationStrategyContext cosc)
        {
            CollectionOperationResult cor = new CollectionOperationResult();
            // Constructs the list node and relationship to the individual
            CreateBaseTriples(cosc, cor.TripleSet);
            INode currentNode = cosc.ListNode;

            for (int i = 0; i < cosc.CollectionProperty.Elements.Count; i++)
            {
                object? element = cosc.CollectionProperty.Elements[i];
                INode targetNode = TargetNodeForElement(element);

                // Starting at the listNode, walk the "next" and see if any of them are the listTargetEntity.Individual.Resource
                INode walkingCurrent = cosc.ListNode;
                bool exists = false;
                while (walkingCurrent != null && walkingCurrent != RdfNilNode)
                {
                    var currentWalkingFirst = WorkingGraph.Graph.GetTriplesWithSubjectPredicate(walkingCurrent, RdfFirstNode).FirstOrDefault();
                    if (targetNode.Equals(currentWalkingFirst?.Object))
                    {
                        exists = true;
                        break;
                    }

                    var currentWalkingRest = WorkingGraph.Graph.GetTriplesWithSubjectPredicate(walkingCurrent, RdfRestNode).FirstOrDefault();
                    walkingCurrent = currentWalkingRest?.Object;
                    if (walkingCurrent != null)
                        currentNode = walkingCurrent;
                    else
                        walkingCurrent = RdfNilNode;
                }

                if (!exists)
                {
                    cor.TripleSet.Assert.Add(new Triple(currentNode, RdfFirstNode, targetNode));

                    // If there are still more items to add
                    if (i < cosc.CollectionProperty.Elements.Count - 1)
                    {
                        var nextNode = WorkingGraph.CreateBlankNode();
                        cor.TripleSet.Assert.Add(new Triple(currentNode, RdfRestNode, nextNode));
                        //cor.TripleSet.Assert.Add(new Triple(nextNode, RdfAUriNode, cosc.Graph.Graph.CreateUriNode(cosc.CollectionProperty.CollectionType)));

                        currentNode = nextNode;
                    }
                    else
                    {
                        cor.TripleSet.Assert.Add(new Triple(currentNode, RdfRestNode, RdfNilNode));
                    }
                }
            }

            // Need to remove the last triple as this will be a bogus rdf:rest to an empty node
            if (cor.TripleSet.Assert.Count == 0)
            {
                var tripleBlank = cor.TripleSet.Assert[1];
                cor.TripleSet.Assert[1] = new Triple(tripleBlank.Subject, tripleBlank.Predicate, RdfNilNode);
            }

            return cor;
        }
    }
}
