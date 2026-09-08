using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.Collections;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;

namespace FEAR.Runtime.KnowledgeGraph.GraphQL.Collections
{
    public class CollectionUpdaterStrategy : BaseCollectionOperationStrategy
    {
        public CollectionUpdaterStrategy(GraphUpdateContext guc) : base(guc) { }
        public override CollectionOperationResult ListOperation(CollectionOperationStrategyContext cosc)
        {
            throw new NotImplementedException("List operations are not yet implemented in the CollectionUpdaterStrategy for GraphQL.");
            CollectionOperationResult cor = new CollectionOperationResult();

            // Maybe we could construct an update that queries for the listNode first?
            // https://afs.github.io/rdf-lists-sparql (Add element to end of list)
            // We could do something similar with "bag" by having a sparql query that finds the node and 'adds' to it.
            // The question being can we do multiple at once, resulting in the need to delete a single, and update a 'chain' for lists.
            // Bags are simpler, as we can just add to the bag with an update query with multiple entries with relevant 'rdf:_#' as we
            // have determined in the code.

            // The 'delete' then 'assert' is going to be the easiest method of implementing this.

            // Construct the 'delete' sparql query to remove the 'collection' from the dataset

            // if list length >= 1

            var listHasItems = @$"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
            DELETE {{{{ 
                    ?elt rdf:rest rdf:nil 
            }}}}
            INSERT {{{{ 
                    ?elt rdf:rest {{0}}
            }}}}
            WHERE
            {{{{
                  @subject @predicate ?list .
                  # List of length >= 1
                  ?list rdf:rest* ?elt .
                  ?elt rdf:rest rdf:nil .
                  # ?elt is last cons cell
            }}}}";

            var listIsEmptyWithNil = @$"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
            DELETE {{{{ 
                    @subject @predicate rdf:nil . 
            }}}}
            INSERT {{{{ 
                    @subject @predicate {{0}} 
            WHERE
            {{{{
                    @subject @predicate rdf:nil .
            }}}}";

            List<INode> nodesToAdd = new List<INode>();

            // Determine which nodes are missing from the list by walking it.
            Triple endOfListTriple = null;
            foreach (var element in cosc.CollectionProperty.Elements)
            {
                INode targetNode = null;
                if (element is Entity)
                {
                    var fec = GraphUpdateContext.DuplicateContextForEntity(element as Entity);
                    var listTargetEntity = WorkingGraph.CreateOrUpdateEntity(fec);
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

                // Starting at the listNode, walk the "next" and see if any of them are the listTargetEntity.Individual.Resource
                INode walkingCurrent = cosc.ListNode;

                bool exists = false;
                while (walkingCurrent != null && !(walkingCurrent is IUriNode && (walkingCurrent as IUriNode).Uri == RdfNilNode.Uri))
                {
                    var currentWalkingFirst = WorkingGraph.Graph.GetTriplesWithSubjectPredicate(walkingCurrent, RdfFirstNode).FirstOrDefault();
                    if (targetNode.Equals(currentWalkingFirst?.Object))
                    {
                        exists = true;
                        break;
                    }

                    var currentWalkingRest = WorkingGraph.Graph.GetTriplesWithSubjectPredicate(walkingCurrent, RdfRestNode).FirstOrDefault();
                    walkingCurrent = currentWalkingRest?.Object;

                    if ((walkingCurrent as IUriNode)?.Uri?.ToString() == RdfNilNode.Uri.ToString())
                    {
                        endOfListTriple = currentWalkingRest;
                    }
                }

                if (!exists)
                {
                    nodesToAdd.Add(targetNode);
                }
            }

            string sparqlTempate = listHasItems;
            if (cosc.ListNode == RdfNilNode)
            {
                sparqlTempate = listIsEmptyWithNil;
            }

            if (nodesToAdd.Count > 0)
            {
                if (endOfListTriple != null)
                {
                    // If the list is not empty, we need to remove the rdf:nil and add the new elements to the end of the list.
                    cor.TripleSet.Retract.Add(endOfListTriple);
                }

                // Construct the 'insert' sparql query to add the 'collection' to the dataset
                string template = "[rdf:first {0} ; rdf:rest {1}]";
                string insertStatement = template;
                INode currentNode = endOfListTriple.Subject;
                for (int i = 0; i < nodesToAdd.Count; i++)
                {
                    insertStatement = string.Format(insertStatement, $"@o{i}", i + 1 < nodesToAdd.Count ? template : $"@o{i + 1}");

                    cor.TripleSet.Assert.Add(new Triple(currentNode, RdfFirstNode, nodesToAdd[i]));

                    if (i < nodesToAdd.Count - 1)
                    {
                        INode nextNode = WorkingGraph.Graph.CreateBlankNode();
                        cor.TripleSet.Assert.Add(new Triple(currentNode, RdfRestNode, nextNode));
                        currentNode = nextNode;
                    }
                    else
                    {
                        cor.TripleSet.Assert.Add(new Triple(currentNode, RdfRestNode, RdfNilNode));
                    }
                }

                string formattedQuery = string.Format(sparqlTempate, insertStatement);
                cor.Query = formattedQuery;
                cor.QueryParameters.Add("subject", cosc.TargetEntity.Individual.Resource);
                cor.QueryParameters.Add("predicate", cosc.PropertyUriNode);

                int nodeIndex = 0;
                foreach (var node in nodesToAdd)
                {
                    cor.QueryParameters.Add($"o{nodeIndex}", node);
                    nodeIndex++;
                }

                cor.QueryParameters.Add($"o{nodeIndex}", RdfNilNode);
                cor.RequiresExecute = true;
            }

            return cor;
        }

        public override CollectionOperationResult BagOperation(CollectionOperationStrategyContext cosc)
        {
            throw new NotImplementedException("List operations are not yet implemented in the CollectionUpdaterStrategy for GraphQL.");
            CollectionOperationResult cor = new CollectionOperationResult();

            // Maybe we could construct an update that queries for the listNode first?
            // https://afs.github.io/rdf-lists-sparql (Add element to end of list)
            // We could do something similar with "bag" by having a sparql query that finds the node and 'adds' to it.
            // The question being can we do multiple at once, resulting in the need to delete a single, and update a 'chain' for lists.
            // Bags are simpler, as we can just add to the bag with an update query with multiple entries with relevant 'rdf:_#' as we
            // have determined in the code.

            IRealGraph graph = cosc.Graph;
            Dictionary<int, INode> nodesToAdd = new Dictionary<int, INode>();

            // We use a for loop starting at 1, as the rdf:_# is 1 based, not 0 based.
            for (int i = 1; i <= cosc.CollectionProperty.Elements.Count; i++)
            {
                var element = cosc.CollectionProperty.Elements[i - 1];
                INode targetNode = null;
                if (element is Entity)
                {
                    var fec = GraphUpdateContext.DuplicateContextForEntity(element as Entity);
                    var listTargetEntity = WorkingGraph.CreateOrUpdateEntity(fec);
                    targetNode = listTargetEntity.Individual.Resource;
                }
                else
                {
                    if (element is Uri)
                    {
                        targetNode = graph.Graph.CreateUriNode(element as Uri);
                    }
                    else
                    {
                        // This is a literal, so we need to add it to the list of nodes to add.
                        targetNode = graph.Graph.CreateLiteralNode(element.ToString());
                    }
                }

                // See if there is a triple for the list node and the entity, don't care about the "number"
                var triplesWithSubjectObject = graph.Graph.GetTriplesWithSubjectObject(cosc.ListNode, targetNode);
                if (!triplesWithSubjectObject.Any())
                {
                    var collectionTriples = graph.Graph.GetTriplesWithSubject(cosc.ListNode).ToList();
                    //collectionTriples.AddRange(cor.TripleSet.Assert.Where(t => t.Subject == cosc.ListNode));
                    int counter = 1;
                    for (; counter <= collectionTriples.Count() + 10; counter++)
                    {
                        var counterUri = graph.Graph.CreateUriNode($"rdf:_{counter}");
                        var currentTriple = collectionTriples.FirstOrDefault(t => (t.Predicate as IUriNode).Uri.ToString() == counterUri.ToString());
                        if (currentTriple == null)
                        {
                            break;
                        }
                    }

                    // Add this to the list of "nodes" to add.
                    nodesToAdd.Add(counter, targetNode);
                }
            }

            if (nodesToAdd.Count > 0)
            {
                // Construct the 'insert' sparql query to add the 'collection' to the dataset
                string template = "?object rdf:_{0} {1} .";
                StringBuilder insertStatement = new StringBuilder();
                foreach (var nta in nodesToAdd)
                {
                    insertStatement.Append(string.Format(template, nta.Key, $"@o{nta.Key}"));
                    cor.TripleSet.Assert.Add(new Triple(cosc.ListNode, graph.Graph.CreateUriNode($"rdf:_{nta.Key}"), nta.Value));
                }

                // <rdf:a rdf:resource="&rdf;Bag" />
                cor.Query = @$"
                PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
                INSERT {{ {insertStatement} }}
                WHERE {{ @subject @predicate ?object .
                
                FILTER(isBlank(?object))
                }}";

                cor.QueryParameters.Add("subject", cosc.TargetEntity.Individual.Resource);
                cor.QueryParameters.Add("predicate", cosc.PropertyUriNode);
                cor.QueryParameters.Add("collectionType", graph.Graph.CreateUriNode(cosc.CollectionProperty.CollectionType));
                foreach (var node in nodesToAdd)
                {
                    cor.QueryParameters.Add($"o{node.Key}", node.Value);
                }

                cor.RequiresExecute = true;
            }

            return cor;
        }
    }
}
