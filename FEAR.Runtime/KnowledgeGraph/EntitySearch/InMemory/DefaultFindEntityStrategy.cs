using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using FEAR.Domain.KnowledgeGraph.TypeService;
using VDS.RDF;

namespace FEAR.Runtime.KnowledgeGraph.EntitySearch.InMemory
{
    public class DefaultFindEntityStrategy : IFindEntityStrategy
    {
        public DefaultFindEntityStrategy(IRealGraph searchGraph, ITypeConversionService typeConversionService)
        {
            SearchGraph = searchGraph;
            TypeConversionService = typeConversionService;
        }

        public ITypeConversionService TypeConversionService { get; }
        public IRealGraph SearchGraph { get; }
        public KGResponse<Entity> FindEntity(GraphUpdateContext entityContext)
        {
            var result = InternalFindEntity(entityContext);
            if (result.IsSuccess)
            {
                // Where multiple candidates satisfy the required conditions, the optional
                // conditions disambiguate: select the candidate satisfying the most of them.
                var best = result.Value.OrderByDescending(r => r.OptionalCount).First();
                return new KGResponse<Entity>().WithSuccess(best.Entity);
            }
            else
            {
                return new KGResponse<Entity>().WithError(result.Message);
            }
        }

        private KGResponse<IList<EntitySearchResult>> InternalFindEntity(GraphUpdateContext entityContext)
        {
            // Based on the entity, find a list of potential entities that may be the one we are looking for.
            var potentialEntities = new List<EntitySearchResult>();

            // Check for required properties first
            var requiredOptions = entityContext.IntermediateEntity.IdentifiedByOptions
                .Where(t => t.IdentifiedByCondition == IdentifiedByCondition.Required)
                .ToList();
            
            foreach (var option in requiredOptions)
            {
                var propertyUri = entityContext.GraphManager.GetUriFromPrefixNotation(option.Property).Value;
                var propertyUriNode = entityContext.TemporaryGraph.GetNodeForUri(propertyUri);
                if (option.IdentifiedByType == IdentifiedByType.Property)
                {
                    // Retrieves the value from the entity that is attempted to be found
                    var value = entityContext.IntermediateEntity.Individual.GetLiteralProperty(propertyUri);

                    // if potentialEntities is empty, we start with needing to find all possible entities that have the literal
                    if (potentialEntities.Count == 0)
                    {
                        foreach (var v in value)
                        {
                            entityContext.GraphManager.OntologyGraph.NamespaceMap.ReduceToQName(v.DataType.ToString(), out string qName);
                            var object_value = TypeConversionService.ConvertFromString(v.Value, qName, false);

                            var literal = entityContext.TemporaryGraph.GetOrCreateLiteralNode(object_value.ToString(), v.DataType.ToString()).Value;
                            var subjectsFind = SearchGraph.FindSubjectOfLiteral(literal, option.Property);
                            if (subjectsFind.IsSuccess)
                            {
                                foreach (var subject in subjectsFind.Value)
                                {
                                    var subjectType = subject.Types.FirstOrDefault(t => (t as UriNode)?.Uri.Equals(entityContext.IntermediateEntity.Type) ?? false);
                                    if (subjectType != null)
                                    {
                                        var graphEntity = new Entity()
                                        {
                                            Individual = subject,
                                            Iri = (subject.Resource as IUriNode).Uri,
                                            OwnerGraph = SearchGraph,
                                            Type = (subjectType as UriNode).Uri,
                                            ScriptVariableName = entityContext.IntermediateEntity.ScriptVariableName
                                        };

                                        potentialEntities.Add(new EntitySearchResult() { Entity = graphEntity, RequiredFound = true });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // if potentialEntities is not empty, we start with the potentialEntities
                        potentialEntities.ToList().ForEach(potentialEntity =>
                        {
                            foreach (var v in value)
                            {
                                entityContext.GraphManager.OntologyGraph.NamespaceMap.ReduceToQName(v.DataType.ToString(), out string qName);
                                var object_value = TypeConversionService.ConvertFromString(v.Value, qName, false);

                                var literal = entityContext.TemporaryGraph.GetOrCreateLiteralNode(object_value.ToString(), v.DataType.ToString()).Value;
                                var peList = potentialEntities.ToList();
                                foreach (var pe in peList)
                                {
                                    // PotentiaEntity has the literal property
                                    if (pe.Entity.Individual.TriplesWithSubject.FirstOrDefault(t => t.HasPredicate(propertyUriNode) && (t.Object as ILiteralNode).Value == literal.Value) == null)
                                    {
                                        potentialEntities.Remove(potentialEntity);
                                    }
                                }
                            }
                        });
                    }
                }
                else
                {
                    var guc = entityContext.DuplicateContextForEntity(option.Entity);
                    var rs = InternalFindEntity(guc);
                    if (rs.IsSuccess)
                    {
                        foreach (var e in rs.Value)
                        {
                            if (potentialEntities.Count == 0)
                            {
                                var propertyNode = entityContext.TemporaryGraph.GetNodeForUri(propertyUri);
                                if (propertyNode != null)
                                {
                                    var tripleFind = new KGResponse<IEnumerable<Triple>>().WithSuccess(entityContext.TemporaryGraph.Graph.GetTriplesWithPredicateObject(propertyNode, e.Entity.Individual.Resource));
                                    if (tripleFind.IsSuccess)
                                    {
                                        foreach (var triple in tripleFind.Value)
                                        {
                                            if (triple.Subject as IUriNode == null)
                                                continue;

                                            var graphIndividual = entityContext.TemporaryGraph.GetIndividual(triple.Subject as IUriNode).Value;
                                            var graphEntity = new Entity()
                                            {
                                                Individual = graphIndividual,
                                                Iri = (graphIndividual.Resource as IUriNode).Uri,
                                                OwnerGraph = SearchGraph,
                                                Type = entityContext.IntermediateEntity.Type,
                                                ScriptVariableName = entityContext.IntermediateEntity.ScriptVariableName
                                            };

                                            potentialEntities.Add(new EntitySearchResult() { Entity = graphEntity, RequiredFound = true });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var peList = potentialEntities.ToList();
                                foreach (var pe in peList)
                                {
                                    var propertyNode = entityContext.TemporaryGraph.GetNodeForUri(propertyUri);
                                    var resProperties = SearchGraph.Graph.GetTriplesWithSubjectPredicate(pe.Entity.Individual.Resource, propertyNode);
                                    if (!resProperties.Any(t => t.HasObject(e.Entity.Individual.Resource)))
                                    {
                                        potentialEntities.Remove(pe);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        potentialEntities.Clear();
                    }
                }

                if (potentialEntities.Count == 0)
                    break;
            }

            // Look for optional properties
            if (potentialEntities.Count > 0)
            {

                // Check for required properties first
                var optionalOptions = entityContext.IntermediateEntity.IdentifiedByOptions
                    .Where(t => t.IdentifiedByCondition == IdentifiedByCondition.Optional)
                    .ToList();

                foreach (var option in optionalOptions)
                {
                    var propertyUri = entityContext.GraphManager.GetUriFromPrefixNotation(option.Property).Value;
                    if (option.IdentifiedByType == IdentifiedByType.Property)
                    {
                        var value = entityContext.IntermediateEntity.Individual.GetLiteralProperty(propertyUri);
                        potentialEntities.ToList().ForEach(potentialEntity =>
                        {
                            foreach (var v in value)
                            {
                                var literal = entityContext.TemporaryGraph.GetOrCreateLiteralNode(v.Value, v.DataType.ToString());
                                foreach (var pe in potentialEntities)
                                {
                                    // PotentiaEntity has the literal property
                                    if (pe.Entity.Individual.GetLiteralProperty(option.Property).Any(t => t == literal))
                                    {
                                        pe.OptionalCount++;
                                    }
                                }
                            }
                        });
                    }
                    else
                    {
                        var guc = entityContext.DuplicateContextForEntity(option.Entity);
                        var rs = InternalFindEntity(guc);
                        if (rs.IsSuccess)
                        {
                            foreach (var e in rs.Value)
                            {
                                foreach (var pe in potentialEntities)
                                {
                                    if (pe.Entity.Individual.GetResourceProperty(propertyUri).Any(t => t == e.Entity.Individual.Resource))
                                    {
                                        pe.OptionalCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (potentialEntities.Count > 0)
            {
                // This ensure that the entity has the required 'type' property.

                /////////////////////////////////////////////////////////////////////
                // We could add checks for 'sameAs' or 'equivalentClass' here aswell.
                /////////////////////////////////////////////////////////////////////
                var rdfType = entityContext.GraphManager.GetUriFromPrefixNotation("rdf:type").Value;
                var rdfTypeNode = entityContext.TemporaryGraph.GetNodeForUri(rdfType);
                var entityTypeNode = entityContext.TemporaryGraph.GetNodeForUri(entityContext.IntermediateEntity.Type);
                var potentialEntitiesMatchingType = potentialEntities.Where(pe => pe.Entity.Individual.TriplesWithSubject.FirstOrDefault(t => t.HasPredicate(rdfTypeNode) && t.Object.Equals(entityTypeNode)) != null).ToList();
                if (potentialEntitiesMatchingType.Count > 0)
                {
                    potentialEntities = potentialEntitiesMatchingType;
                    // We need to load the triples for the potential entities into the temporary graph
                    foreach (var pe in potentialEntities)
                    {
                        var subjectTriples = SearchGraph.Graph.GetTriplesWithSubject(pe.Entity.Individual.Resource)
                            .ToList();

                        // Add to the temporary graph
                        foreach (var triple in subjectTriples)
                        {
                            entityContext.TemporaryGraph.Graph.Assert(triple);
                        }

                        var subjectAsObjectTriples = SearchGraph.Graph.GetTriplesWithObject(pe.Entity.Individual.Resource)
                            .Where(t=>t.Subject.NodeType != NodeType.Blank)
                            .ToList();

                        // Add to the temporary graph
                        foreach (var triple in subjectAsObjectTriples)
                        {
                            entityContext.TemporaryGraph.Graph.Assert(triple);
                        }

                        var triplesForParentSubjects = subjectAsObjectTriples.SelectMany(t => SearchGraph.Graph.GetTriplesWithSubject(t.Subject));

                        // Add to the temporary graph
                        foreach (var triple in triplesForParentSubjects)
                        {
                            entityContext.TemporaryGraph.Graph.Assert(triple);
                        }

                        // Find any blank nodes in the triplesForParentSubjects
                        List<INode> blankNodes = triplesForParentSubjects.Where(t => t.Object.NodeType == NodeType.Blank).Select(t=>t.Object).Distinct().ToList();

                        // Get any triples that have the blank nodes as the subject
                        foreach (var blankNode in blankNodes)
                        {
                            var blankNodeTriples = SearchGraph.Graph.GetTriplesWithSubject(blankNode).ToList();
                            foreach (var triple in blankNodeTriples)
                            {
                                entityContext.TemporaryGraph.Graph.Assert(triple);
                            }
                        }
                    }
                }

                return new KGResponse<IList<EntitySearchResult>>().WithSuccess(potentialEntities);
            }

            return new KGResponse<IList<EntitySearchResult>>().WithError("Could not find an entity matching required/optional/entity fields.");
        }
    }
}
