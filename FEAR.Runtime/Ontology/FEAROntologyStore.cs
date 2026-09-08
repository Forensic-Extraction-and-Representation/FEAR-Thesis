using FEAR.Domain.Ontology;
using VDS.RDF.Ontology;

namespace FEAR.Runtime.Ontology
{
    public class FEAROntologyStore :IFEAROntologyStore
    {
        private OntologyGraph _ontology = new OntologyGraph();
        public OntologyGraph Ontology => _ontology;
        public bool OntologyUpdated { get; private set; } = false;
        protected string OntologyAsTurtle = null;
        public void Merge(OntologyGraph ontology)
        {
            _ontology.Merge(ontology);
            OntologyUpdated = true;
        }

        public string GetOntology()
        {
            if (!OntologyUpdated && !String.IsNullOrEmpty(OntologyAsTurtle))
                return OntologyAsTurtle;
            else
            {
                var ttlWriter = new VDS.RDF.Writing.CompressingTurtleWriter();
                ttlWriter.PrettyPrintMode = true;
                ttlWriter.HighSpeedModePermitted = false;
                ttlWriter.CompressionLevel = VDS.RDF.Writing.WriterCompressionLevel.High;
                ttlWriter.PrettyPrintMode = true;
                System.IO.StringWriter strWriter = new System.IO.StringWriter();
                ttlWriter.Save(_ontology, strWriter);
                OntologyAsTurtle = strWriter.ToString();
                return OntologyAsTurtle;
            }
        }
    }
}
