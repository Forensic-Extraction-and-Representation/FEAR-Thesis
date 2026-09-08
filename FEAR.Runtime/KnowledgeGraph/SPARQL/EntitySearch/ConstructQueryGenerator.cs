using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using FEAR.Domain.KnowledgeGraph.TypeService;
using FEAR.Runtime.KnowledgeGraph.EntitySearch;
using System.Text;
using VDS.RDF.Query;

namespace FEAR.Runtime.KnowledgeGraph.Sparql.EntitySearch
{

    public class ConstructQueryGenerator : BaseConstructQueryGenerator<SparqlParameterizedString>
    {
        protected override string QueryParameterPrefix => "@";
        protected override string QueryStatementTripleTerminator => ".";

        public ConstructQueryGenerator(IRemoteGraph<SparqlParameterizedString> graph, ITypeConversionService typeConversionService)
        :base(graph, typeConversionService)
        {
        }

        public override SparqlParameterizedString ConstructLiteralEntityLoadQuery(ICollection<GraphUpdateEntity> leafs)
        {
            GenerationState genState = new GenerationState(TypePredicate, TypeUriNode);
            foreach (GraphUpdateEntity leaf in leafs)
            {
                genState.WhereClauses.AppendLine($"OPTIONAL {{");
                AddEntityTypeLoading(leaf, genState);

                int i = 0;
                Entity leafEntity = leaf.Entity;
                foreach (var identifiedByList in new[]{
                        leafEntity.IdentifiedByOptions.Where(t=>t.IdentifiedByCondition == IdentifiedByCondition.Required),
                        leafEntity.IdentifiedByOptions.Where(t=>t.IdentifiedByCondition == IdentifiedByCondition.Optional) }
                )
                {
                    if (identifiedByList.Count() == 0)
                        continue;

                    // These are the properties of the entity
                    foreach (IdentifiedByOption? ibo in identifiedByList)
                    {
                        if (ibo.IdentifiedByType != IdentifiedByType.Property)
                            throw new Exception("Leaf contains an Entity when it should only contain Properties");

                        var propertyUri = UriFor(ibo.Property);
                        var propertyNode = UriNodeFor(propertyUri);
                        genState.AddQueryParameter($"@p{leaf.EntityQueryIdentifier}", UriNodeFor(ibo.Property));

                        if (ibo.IdentifiedByCondition == IdentifiedByCondition.Optional)
                        {
                            genState.WhereClauses.AppendLine($"OPTIONAL {{");
                        }

                        foreach (var x in leafEntity.GetPropertyValuesByName(ibo.Property))
                        {
                            string objectIdentifier = $"@o{leaf.EntityQueryIdentifier}i{i++}";
                            genState.AddQueryParameter(objectIdentifier, LiteralNodeFor(x.Value, x.PropertyType));

                            genState.ConstructEntries.AppendLine($"?{leaf.EntityQueryIdentifier} @p{leaf.EntityQueryIdentifier} {objectIdentifier} .");
                            genState.WhereClauses.AppendLine($"?{leaf.EntityQueryIdentifier} @p{leaf.EntityQueryIdentifier} {objectIdentifier} .");
                        }

                        if (ibo.IdentifiedByCondition == IdentifiedByCondition.Optional)
                        {
                            genState.WhereClauses.AppendLine($"}}");
                        }

                    }

                }
                genState.WhereClauses.AppendLine($"}}");
            }

            return GenerateQuery(genState);
        }
        
        public override SparqlParameterizedString ConstructNodeEntityLoadQuery(Dictionary<IdentifiedEntity, int> flattenedTree, IList<GraphUpdateCollection> collections, int maxDepth)
        {
            GenerationState overallState = new GenerationState(TypePredicate, TypeUriNode);
            Dictionary<string, string> propertyTypeParameters = new Dictionary<string, string>();
            for (int i = maxDepth; i >= 0; i--)
            {
                foreach (IdentifiedEntity entityNode in flattenedTree.Where(t => t.Value == i).Select(t => t.Key))
                {
                    GenerationState genState = new GenerationState(TypePredicate, TypeUriNode);
                    genState.WhereClauses.AppendLine("OPTIONAL {");
                    AddEntityTypeLoading(entityNode.Entity, genState);

                    int o = 0;
                    foreach (GraphUpdateIdentifiableNode idEntity in entityNode.Entity.IdentifyingNodes)
                    {
                        string predicateName = $"@pn{entityNode.Entity.EntityQueryIdentifier}_i{o++}";

                        var propertyUri = UriFor(idEntity.ParentReference.PropertyName);
                        var propertyNode = UriNodeFor(propertyUri);

                        genState.AddQueryParameter(predicateName, propertyNode);

                        if (idEntity is GraphUpdateLiteral)
                        {
                            GraphUpdateLiteral literal = idEntity as GraphUpdateLiteral;
                            var x = literal.NodeValue;
                            string objectIdentifier = $"@on{entityNode.Entity.EntityQueryIdentifier}i{o++}";
                            genState.AddQueryParameter(objectIdentifier, LiteralNodeFor(x.Value, x.PropertyType));

                            genState.ConstructEntries.AppendLine($"?{entityNode.Entity.EntityQueryIdentifier} {predicateName} {objectIdentifier} .");
                            genState.WhereClauses.AppendLine($"?{entityNode.Entity.EntityQueryIdentifier} {predicateName} {objectIdentifier} .");
                        }
                        else if (idEntity is GraphUpdateEntity)
                        {
                            GraphUpdateEntity guc = idEntity as GraphUpdateEntity;

                            if (guc.EntityGraphReference?.EntityGraphUri != null)
                            {
                                string entityUriVariable = $"@{guc.EntityQueryIdentifier}_Uri";
                                genState.AddQueryParameter(entityUriVariable, UriNodeFor(guc.EntityGraphReference.EntityGraphUri));
                                genState.ConstructEntries.AppendLine($"?{entityNode.Entity.EntityQueryIdentifier} {predicateName} {entityUriVariable} .");
                                genState.WhereClauses.AppendLine($"?{entityNode.Entity.EntityQueryIdentifier} {predicateName} {entityUriVariable} .");
                            }
                            else
                            {
                                genState.ConstructEntries.AppendLine($"?{entityNode.Entity.EntityQueryIdentifier} {predicateName} ?{guc.EntityQueryIdentifier} .");
                                genState.WhereClauses.AppendLine($"?{entityNode.Entity.EntityQueryIdentifier} {predicateName} ?{guc.EntityQueryIdentifier} .");
                            }
                        }
                    }

                    genState.WhereClauses.AppendLine("}");

                    overallState.Merge(genState);
                }
            }

            foreach (GraphUpdateCollection col in collections)
            {
                if (col.Collection.CollectionType == "rdf:List")
                    continue;

                GenerationState genState = new GenerationState(RdfAPredicate, RdfAUriNode);

                genState.AddQueryParameter(RdfFirstPredicate, RdfFirstUriNode);
                genState.AddQueryParameter(RdfRestPredicate, RdfRestUriNode);

                genState.WhereClauses.AppendLine("OPTIONAL {");

                string collectionPredicateName = $"@pn{col.CollectionQueryIdentifier}";
                genState.AddQueryParameter(collectionPredicateName, UriNodeFor(col.ParentReference.PropertyName));
                genState.ConstructEntries.AppendLine($"?{col.ParentReference.ParentEntity.EntityQueryIdentifier} {collectionPredicateName} ?{col.CollectionQueryIdentifier} .");
                genState.WhereClauses.AppendLine($"?{col.ParentReference.ParentEntity.EntityQueryIdentifier} {collectionPredicateName} ?{col.CollectionQueryIdentifier} .");

                // Bag operations
                genState.ConstructEntries.AppendLine($"?{col.CollectionQueryIdentifier} ?c{col.CollectionQueryIdentifier}_element_p ?c{col.CollectionQueryIdentifier}_element_v .");
                genState.WhereClauses.AppendLine($"?{col.CollectionQueryIdentifier} ?c{col.CollectionQueryIdentifier}_element_p ?c{col.CollectionQueryIdentifier}_element_v .");

                genState.WhereClauses.AppendLine("}");
                overallState.Merge(genState);
            }

            return GenerateQuery(overallState);
        }

        public override SparqlParameterizedString LoadListCollections(GraphUpdateCollection collection)
        {
            var propertyUri = UriFor(collection.ParentReference.PropertyName);
            var propertyNode = UriNodeFor(propertyUri);
            
            var subjectNode = UriNodeFor(collection.ParentReference.ParentEntity.EntityGraphReference.EntityGraphUri);

            SparqlParameterizedString query = new SparqlParameterizedString("""
                PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
                CONSTRUCT { ?u rdf:first ?el }
                    WHERE {
                      @subject @predicate ?u .
                      ?u rdf:rest*/rdf:first ?el .
                }
            """);

            query.SetParameter("subject", subjectNode);
            query.SetParameter("predicate", propertyNode);

            return query;

        }

        private SparqlParameterizedString GenerateQuery(GenerationState genState)
        {
            StringBuilder queryBuilder = new StringBuilder();

            if (genState.ConstructEntries.Length > 0 && genState.WhereClauses.Length > 0)
            {
                queryBuilder.AppendLine("CONSTRUCT {");
                queryBuilder.Append(genState.ConstructEntries);
                queryBuilder.AppendLine("}");

                queryBuilder.AppendLine("WHERE {");
                queryBuilder.Append(genState.WhereClauses);
                queryBuilder.AppendLine("}");

                SparqlParameterizedString sps = new SparqlParameterizedString(queryBuilder.ToString());
                foreach (var item in genState.QueryParameters)
                {
                    sps.SetParameter(item.Key, item.Value);
                }

                return sps;
            }

            return null;
        }

        protected void AddEntityTypeLoading(GraphUpdateEntity leaf, GenerationState genState)
        {
            if (!genState.EntityCreations.Contains(leaf.EntityQueryIdentifier))
            {
                genState.EntityCreations.Add(leaf.EntityQueryIdentifier);
                genState.ConstructEntries.AppendLine($"?{leaf.EntityQueryIdentifier} {TypePredicate} ?{leaf.EntityQueryIdentifier}_type .");
                genState.WhereClauses.AppendLine($"?{leaf.EntityQueryIdentifier} {TypePredicate} ?{leaf.EntityQueryIdentifier}_type .");
            }
        }
    }
}