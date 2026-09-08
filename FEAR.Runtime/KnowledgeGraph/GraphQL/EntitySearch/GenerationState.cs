using System.Text;
using VDS.RDF;

namespace FEAR.Runtime.KnowledgeGraph.GraphQL.EntitySearch
{
    public class GenerationState
    {
        public StringBuilder ConstructEntries = new StringBuilder();
        public StringBuilder WhereClauses = new StringBuilder();

        public Dictionary<string, INode> QueryParameters = new Dictionary<string, INode>();

        public List<string> EntityCreations = new List<string>();

        public GenerationState(string typePredicate, INode typeUriNode)
        {
            QueryParameters.Add(typePredicate, typeUriNode);
        }

        public void Merge(GenerationState genState)
        {
            ConstructEntries.Append(genState.ConstructEntries);
            WhereClauses.Append(genState.WhereClauses);

            foreach (var item in genState.QueryParameters)
            {
                if (!QueryParameters.ContainsKey(item.Key))
                    QueryParameters.Add(item.Key, item.Value);
            }

            foreach (var item in genState.EntityCreations)
            {
                if (!EntityCreations.Contains(item))
                    EntityCreations.Add(item);
            }
        }

        public void AddQueryParameter(string objectIdentifier, INode literalNode)
        {
            if (!QueryParameters.ContainsKey(objectIdentifier))
                QueryParameters.Add(objectIdentifier, literalNode);
        }
    }
}