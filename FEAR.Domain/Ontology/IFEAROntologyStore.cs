using VDS.RDF.Ontology;

namespace FEAR.Domain.Ontology
{
    public interface IFEAROntologyStore
    {
        string GetOntology();
        void Merge(OntologyGraph ontology);
    }
}
