using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphDB.Cypher;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using FEAR.Domain.KnowledgeGraph.TypeService;
using FEAR.Runtime.KnowledgeGraph.EntitySearch;
using System.Collections;
using System.Text;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace FEAR.Runtime.KnowledgeGraph.Cypher.EntitySearch
{
    public class GenerationState
    {
        public class EntitySearchConditions
        {
            /// <summary>
            /// This is the identifier within the query for the entity.
            /// It does not represent a parameter name, but rather a variable name used in the Cypher query.
            /// </summary>
            public string EntityQueryIdentifier { get; set; }

            /// <summary>
            /// Represents the raw URI of the entity type.
            /// </summary>
            public string EntityTypeUri { get; set; }

            /// <summary>
            /// Contains the Uri and value pairs for conditions that will be applied to the entity.
            /// </summary>
            public Dictionary<Uri, object> Conditions = new Dictionary<Uri, object>();
            /// <summary>
            /// Denotes the variable name of the related entity in the Cypher query that represents the RDF type node/uri of the entity.
            /// </summary>
            public string EntityTypeQueryIdentifier => $"{EntityQueryIdentifier}_t";
            public string MatchClause(Uri typeUri) => $"MATCH ({EntityQueryIdentifier}:Resource)-[:`{typeUri.ToString()}`]->({EntityTypeQueryIdentifier})";
            public string TypeMatchClause() => $"{EntityQueryIdentifier}_t.uri = \"{EntityTypeUri}\"";

            public string ReturnStatement => $@"
                WITH {EntityQueryIdentifier}, properties({EntityQueryIdentifier}) AS props
                UNWIND keys(props) AS k
                WITH {EntityQueryIdentifier}.uri AS subject, k AS predicate, props[k] AS val
                WHERE predicate <> ""uri""
                RETURN subject, predicate, val AS object, ""PROP"" as relationType
            ";
        }

        public Dictionary<string, object> QueryParameters = new Dictionary<string, object>();
        public List<EntitySearchConditions> SearchEntities = new List<EntitySearchConditions>();

        public GenerationState()
        {
        }

        public void AddQueryParameter(string objectIdentifier, object literalNode)
        {
            if (!QueryParameters.ContainsKey(objectIdentifier))
            {
                QueryParameters.Add(objectIdentifier, literalNode);
            }
        }
    }

    public class ConstructQueryGenerator : BaseConstructQueryGenerator<CypherParameterizedString>
    {
        protected override string QueryParameterPrefix => "$";
        protected override string QueryStatementTripleTerminator => "";

        public ConstructQueryGenerator(IRemoteGraph<CypherParameterizedString> graph, ITypeConversionService typeConversionService)
        : base(graph, typeConversionService)
        {
        }

        public override CypherParameterizedString ConstructLiteralEntityLoadQuery(ICollection<GraphUpdateEntity> leafs)
        {
            List<string> e;
            GenerationState genState = new GenerationState();

            foreach (GraphUpdateEntity leaf in leafs)
            {
                GenerationState.EntitySearchConditions esc = new GenerationState.EntitySearchConditions();
                esc.EntityQueryIdentifier = leaf.EntityQueryIdentifier;
                esc.EntityTypeUri = leaf.Entity.Type.ToString();
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

                        foreach (var x in leafEntity.GetPropertyValuesByName(ibo.Property))
                        {
                            string objectIdentifier = $"o{leaf.EntityQueryIdentifier}i{i}";
                            var valueLiteral = x.Value;
                            genState.AddQueryParameter(objectIdentifier, valueLiteral);

                            esc.Conditions.Add(propertyNode.Uri, QueryParameterPrefix + objectIdentifier);
                            i++;
                        }
                    }
                }

                genState.SearchEntities.Add(esc);
            }

            return GenerateQuery(genState);
        }
        class EntityName
        {
            public string Name { get; set; }
            public bool WasAdded { get; set; }

            public EntityName(string name, bool wasAdded)
            {
                Name = name;
                WasAdded = wasAdded;
            }
            public override string ToString()
            {
                return Name;
            }
        }

        public override CypherParameterizedString ConstructNodeEntityLoadQuery(Dictionary<IdentifiedEntity, int> flattenedTree, IList<GraphUpdateCollection> collections, int maxDepth)
        {
            CypherParameterizedString finalQuery = new CypherParameterizedString("");
            StringBuilder queryBuilder = new StringBuilder();
            List<string> queryWithStatements = new List<string>();
            List<string> returnStatementList = new List<string>();
            HashSet<string> addedEntities = new HashSet<string>();

            for (int i = maxDepth; i >= 0; i--)
            {
                foreach (IdentifiedEntity entityNode in flattenedTree.Where(t => t.Value == i).Select(t => t.Key))
                {
                    bool addResourceSuffix = addedEntities.Contains(entityNode.Entity.EntityQueryIdentifier);
                    EntityName entityName = getQueryIdentifier(entityNode.Entity.EntityQueryIdentifier);

                    bool isRoot = i == 0;

                    int o = 0;
                    foreach (GraphUpdateIdentifiableNode idEntity in entityNode.Entity.IdentifyingNodes)
                    {
                        GenerationState.EntitySearchConditions esc = new GenerationState.EntitySearchConditions();
                        esc.EntityQueryIdentifier = entityNode.Entity.EntityQueryIdentifier;
                        var propertyUri = UriFor(idEntity.ParentReference.PropertyName);

                        if (idEntity is GraphUpdateLiteral)
                        {
                            GraphUpdateLiteral literal = idEntity as GraphUpdateLiteral;
                            var x = literal.NodeValue;
                            string objectIdentifier = $"{QueryParameterPrefix}on{entityNode.Entity.EntityQueryIdentifier}i{o++}";
                            finalQuery.Parameters.Add(objectIdentifier, x.Value);

                            queryBuilder.AppendLine($"WHERE {entityNode.Entity.EntityQueryIdentifier}.`{propertyUri}` = {objectIdentifier}");
                        }
                        else if (idEntity is GraphUpdateEntity)
                        {
                            GraphUpdateEntity guc = idEntity as GraphUpdateEntity;

                            if (guc.EntityGraphReference?.EntityGraphUri != null)
                            {
                                string entityUriVariable = $"{guc.EntityQueryIdentifier}_Uri";
                                finalQuery.Parameters.Add(entityUriVariable, UriNodeFor(guc.EntityGraphReference.EntityGraphUri).Uri.ToString());
                                queryBuilder.AppendLine($"WHERE EXISTS {{ MATCH ({entityName})-[:`{propertyUri}`]->({entityName}_t {{ uri: {QueryParameterPrefix}{entityUriVariable}}}) }}");
                            }
                            else
                            {
                                EntityName nestedEntityIdentifier = getQueryIdentifier(guc.EntityQueryIdentifier);
                                queryBuilder.AppendLine($"WHERE EXISTS {{ MATCH ({entityName})-[:`{propertyUri}`]->({nestedEntityIdentifier}) }}");
                                if (nestedEntityIdentifier.WasAdded)
                                    appendQueryEntityLoading(nestedEntityIdentifier.Name);
                            }
                        }
                    }

                    if (entityName.WasAdded)
                        appendQueryEntityLoading(entityName.Name);


                    if (addResourceSuffix)
                        addedEntities.Add(entityNode.Entity.EntityQueryIdentifier);

                    queryBuilder.AppendLine();
                }
            }

            /*
            foreach (GraphUpdateCollection col in collections)
            {
                /**
                Something like the following Cypher may be usefule for loading collection lists.
                We should already have all the entities from the previous queries (literals and node entities)
                This query is just to loading the items in the list.

                MATCH path = (head:Resource)-[:`http://www.w3.org/1999/02/22-rdf-syntax-ns#rest`*0..]->(n)
                WHERE head.uri = 'bnode://mylist1'
                WITH nodes(path) AS listNodes
                UNWIND listNodes AS listNode
                MATCH (listNode)-[:`http://www.w3.org/1999/02/22-rdf-syntax-ns#first`]->(item)
                RETURN item.uri
                ** /
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
            */

            void appendQueryEntity(string entityName)
            {
                queryBuilder.AppendLine($"OPTIONAL MATCH ({entityName}:Resource)");

            }
            void appendQueryEntityLoading(string entityName)
            {
                // This is for loading additional properties/relationships of the entity, this is not related to any filtering/searching
                queryBuilder.AppendLine($"OPTIONAL MATCH({entityName})-[{entityName}_predicate]->({entityName}_target:Resource)");
                queryBuilder.AppendLine("WITH ");
                queryBuilder.AppendLine($"{entityName},");
                queryBuilder.AppendLine($"properties({entityName}) as {entityName}_props,");
                queryBuilder.AppendLine($" COLLECT(DISTINCT [type({entityName}_predicate), {entityName}_target.uri]) as {entityName}_rels");

                if (queryWithStatements.Count > 0)
                {
                    queryBuilder.AppendLine(", " + string.Join(",", queryWithStatements));
                }

                queryWithStatements.Add($"{entityName}");
                queryWithStatements.Add($"{entityName}_props");
                queryWithStatements.Add($"{entityName}_rels");

                returnStatementList.Add($"{entityName}.uri AS {entityName}_uri, {entityName}_props, {entityName}_rels");
            }

            EntityName getQueryIdentifier(string name)
            {
                if (addedEntities.Contains(name))
                    return new EntityName(name, false);
                else
                {
                    addedEntities.Add(name);
                    appendQueryEntity(name);
                    return new EntityName(name, true);
                }
            }

            finalQuery.Query = queryBuilder.ToString();
            finalQuery.Query += "WITH " + string.Join(",", queryWithStatements) + "\r\n";
            finalQuery.Query += "RETURN " + string.Join(",", returnStatementList);
            return finalQuery;
        }

        public override CypherParameterizedString LoadListCollections(GraphUpdateCollection collection)
        {
            /**
            Something like the following Cypher may be usefule for loading collection lists.
            We should already have all the entities from the previous queries (literals and node entities)
            This query is just to loading the items in the list.

            MATCH path = (head:Resource)-[:`http://www.w3.org/1999/02/22-rdf-syntax-ns#rest`*0..]->(n)
            WHERE head.uri = 'bnode://mylist1'
            WITH nodes(path) AS listNodes
            UNWIND listNodes AS listNode
            MATCH (listNode)-[:`http://www.w3.org/1999/02/22-rdf-syntax-ns#first`]->(item)
            RETURN item.uri
            **/

            var propertyUri = UriFor(collection.ParentReference.PropertyName);
            var propertyNode = UriNodeFor(propertyUri);

            var subjectNode = UriNodeFor(collection.ParentReference.ParentEntity.EntityGraphReference.EntityGraphUri);

            CypherParameterizedString query = new CypherParameterizedString("""
                PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
                CONSTRUCT { ?u rdf:first ?el }
                    WHERE {
                      @subject @predicate ?u .
                      ?u rdf:rest*/rdf:first ?el .
                }
            """);

            query.Parameters.Add("subject", subjectNode);
            query.Parameters.Add("predicate", propertyNode);

            return query;
        }

        private CypherParameterizedString GenerateQuery(GenerationState genState)
        {
            StringBuilder queryBuilder = new StringBuilder();

            bool requiresUnion = false;
            foreach (var item in genState.SearchEntities)
            {
                if (requiresUnion)
                    queryBuilder.AppendLine("UNION");

                StringBuilder entityMatchBuilder = new StringBuilder();

                entityMatchBuilder.AppendLine(item.MatchClause((TypeUriNode as UriNode).Uri));
                entityMatchBuilder.AppendLine("WHERE " + item.TypeMatchClause());
                if (item.Conditions.Count > 0)
                {
                    // Simplified version:
                    if (item.Conditions.Count > 0)
                    {
                        var conditions = item.Conditions.Select(condition => $"{item.EntityQueryIdentifier}.`{condition.Key}` = {condition.Value}");
                        entityMatchBuilder.AppendLine("AND " + string.Join(" AND ", conditions));
                    }
                }

                queryBuilder.AppendLine(entityMatchBuilder.ToString());
                queryBuilder.AppendLine(item.ReturnStatement);
                queryBuilder.AppendLine("UNION");
                queryBuilder.AppendLine(entityMatchBuilder.ToString());
                queryBuilder.AppendLine($"RETURN {item.EntityQueryIdentifier}.uri AS subject, \"{(TypeUriNode as UriNode).Uri}\" AS predicate, {item.EntityTypeQueryIdentifier}.uri as object, \"REL\" as relationType");

                requiresUnion = true;
            }

            CypherParameterizedString sps = new CypherParameterizedString(queryBuilder.ToString());
            foreach (var item in genState.QueryParameters)
            {
                sps.Parameters.Add(item.Key, item.Value);
            }

            return sps;
        }
    }
}