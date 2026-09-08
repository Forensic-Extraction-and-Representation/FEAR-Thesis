using VDS.RDF.Query;

namespace FEAR.Domain.Agents.Querying
{
    public class GraphResult
    {
        public bool Graphable { get; set; } = true;
        public VDS.RDF.Graph Graph { get; set; } = new VDS.RDF.Graph();

        public SparqlResultSet ResultSet { get; set; } = new SparqlResultSet();
    }
}
