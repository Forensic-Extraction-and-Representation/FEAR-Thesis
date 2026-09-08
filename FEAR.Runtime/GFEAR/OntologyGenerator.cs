using FEAR.Domain.SWRL;
using FEAR.GFEAR.Transpiler;
using FEAR.RFEAR.Transpiler;
using FEAR.Runtime.Compiler;
using FEAR.Runtime.TranspilerServices;
using System.Data;
using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Writing;

namespace FEAR.Runtime.GFEAR
{
    /// <summary>
    /// Generates an ontology (OWL/RDF) representation from the types and properties discovered during GFEAR transpilation.
    ///
    /// This generator is used in the context of graph codify scripts that process data from a queue.
    /// The data to be described in the ontology can be either:
    ///   - An artifact directly (raw evidence or input data)
    ///   - The result of a collector (CFEAR) script
    ///
    /// The ontology is built from the <see cref="TypeManager"/> in the <see cref="GFEARTranspileContext"/>,
    /// ensuring that all entity types, properties, and prefixes—regardless of whether they originate from
    /// direct artifacts or collector results—are represented for downstream knowledge graph processing.
    /// </summary>
    public class OntologyGenerator
    {
        private IUriNode owlDtProp(OntologyGraph _og) => GetNode("owl:DatatypeProperty", _og);
        private IUriNode owlObjProp(OntologyGraph _og) => GetNode("owl:ObjectProperty", _og);
        private IUriNode owlClass(OntologyGraph _og) => GetNode("owl:Class", _og);

        /// <summary>
        /// Generates the ontology from the provided transpile context, including all entity types and properties
        /// discovered from queued data (artifacts or collector results) for graph codification.
        /// </summary>
        /// <param name="context">The transpile context containing type and property metadata.</param>
        public OntologyGraph GenerateOntology(GFEARTranspileContext context)
        {
             OntologyGraph _ontology = new OntologyGraph();
            foreach (var pf in context.TypeManager.Prefixes)
            {
                var pfUri = new Uri(pf.Value);
                if (!_ontology.NamespaceMap.HasNamespace(pf.Key))
                    _ontology.NamespaceMap.AddNamespace(pf.Key, pfUri);
                else
                {
                    var ns = _ontology.NamespaceMap.GetNamespaceUri(pf.Key);
                    if (!ns.Equals(pfUri))
                    {
                        _ontology.NamespaceMap.RemoveNamespace(pf.Key);
                        _ontology.NamespaceMap.AddNamespace(pf.Key, pfUri);
                    }
                }
            }

            foreach (var st in context.TypeManager.EntityTypes)
            {
                EntityTypeMeta etm = st.Value;
                var entityNode = GetNode(etm.Type, _ontology);
                OntologyClass oc = new OntologyClass(entityNode, _ontology);
                oc.AddResourceProperty(GetNode("rdf:type", _ontology).Uri, owlClass(_ontology), true);

                foreach (var x in etm.PropertyTypes)
                {
                    if (x.Value is CollectionPropertyTypeMeta)
                        continue;

                    PropertyTypeMeta ptm = x.Value;
                    var propNode = GetNode(ptm.Name, _ontology);
                    
                    OntologyProperty op = _ontology.AllProperties.FirstOrDefault(t => t.Resource == propNode);
                    if (op == null)
                        op = new OntologyProperty(propNode, _ontology);

                    op.AddResourceProperty(GetNode("rdf:type", _ontology).Uri, ptm.RelationType == RelationType.DataType ? owlDtProp(_ontology) : owlObjProp(_ontology), true);
                    op.AddDomain(oc);
                    op.AddRange(GetNode(ptm.Type, _ontology));
                }
            }

            return _ontology;
        }

        public OntologyGraph GenerateOntology(RuleSetDefinitionBlock rsd, Domain.FEARCompilationRequest options)
        {
            OntologyGraph _ontology = new OntologyGraph();
            foreach (var pf in rsd.OntologyStatements)
            {
                try
                {
                    var pfUri = new Uri(pf.Uri);
                    if (!_ontology.NamespaceMap.HasNamespace(pf.Prefix))
                        _ontology.NamespaceMap.AddNamespace(pf.Prefix, pfUri);
                    else
                    {
                        var ns = _ontology.NamespaceMap.GetNamespaceUri(pf.Prefix);
                        if (!ns.Equals(pfUri))
                        {
                            _ontology.NamespaceMap.RemoveNamespace(pf.Prefix);
                            _ontology.NamespaceMap.AddNamespace(pf.Prefix, pfUri);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CompilerTelemetry.SendCompilationTelemetrySignal($"Error processing ontology prefix '{pf.Prefix}' with URI '{pf.Uri}': {ex.Message}", options.TelemetrySignalService);
                    CompilerTelemetry.SendCompilationTelemetrySignal($"Stack Trace: {ex.StackTrace}", options.TelemetrySignalService);
                }
            }
            foreach (var rule in rsd.RuleDefinitions)
            {
                Dictionary<string, OntologyClass> classes = new Dictionary<string, OntologyClass>();
                foreach (var t in new List<Atom>[] { rule.ParsedRule.Antecedent, rule.ParsedRule.Consequent }) {
                    foreach (var ant in t)
                    {
                        if (ant is ClassAtom ca)
                        {
                            var classNode = GetNode(ca.Iri, _ontology);
                            OntologyClass oc = new OntologyClass(classNode, _ontology);
                            oc.AddResourceProperty(GetNode("rdf:type", _ontology).Uri, owlClass(_ontology), true);
                            if (!classes.ContainsKey(ca.Variables[0].Name))
                                classes[ca.Variables[0].Name] = oc;
                        }
                        else if (ant is PropertyAtom pa)
                        {
                            var propNode = GetNode(pa.Iri, _ontology);
                            OntologyProperty op = _ontology.AllProperties.FirstOrDefault(t => t.Resource == propNode);
                            if (op == null)
                                op = new OntologyProperty(propNode, _ontology);
                            var subjectUri = ant.Variables[0];
                            var objectUri = ant.Variables[1];

                            if (subjectUri.Type == VariableTypeEnum.Variable)
                            {
                                // We know that the subject has the predicate property (op) that we can add to the ontology
                                if (classes.ContainsKey(subjectUri.Name))
                                {
                                    var subjectClass = classes[subjectUri.Name];
                                    op.AddDomain(subjectClass);
                                }
                            }

                            if (objectUri.Type == VariableTypeEnum.Variable)
                            {
                                if (classes.ContainsKey(objectUri.Name))
                                {
                                    var objectClass = classes[objectUri.Name];
                                    op.AddRange(objectClass);
                                }
                            }
                        }
                    }
                }
            }
            return _ontology;
        }

        /// <summary>
        /// Resolves or creates a URI node for the given identifier (QName or URI).
        /// </summary>
        private IUriNode GetNode(string identifier, OntologyGraph ontology)
        {
            var uri = ontology.ResolveQName(identifier);
            return ontology.GetUriNode(uri) ?? ontology.CreateUriNode(uri);
        }

        /// <summary>
        /// Writes the generated ontology to the provided <see cref="TextWriter"/> in Turtle format.
        /// </summary>
        /// <param name="sw">The text writer to output the ontology.</param>
        public void WriteTo(TextWriter sw, OntologyGraph ontology)
        {
            CompressingTurtleWriter tw = new CompressingTurtleWriter();
            tw.DefaultNamespaces.Clear();
            tw.CompressionLevel = WriterCompressionLevel.Default;
            tw.Save(ontology, sw, true);
        }
    }
}
