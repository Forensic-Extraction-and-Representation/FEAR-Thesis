using FEAR.Domain.RuleSet;
using FEAR.GFEAR;
using FEAR.Runtime.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEAR.Runtime.RFEAR
{
    public abstract class FEARRuleBase : IRFEARRule
    {
        public abstract string RuleName { get; }
        public abstract string RuleType { get; }
        public abstract string SparqlInsertQuery { get; }
        public abstract string SparqlConstructQuery { get; }
        public abstract string SwrlRule { get; }
        public abstract List<string> DependsOn { get; }
    }

    public abstract class FEARRuleSetBase : FEARLanguageBase, IFEARRuleSet
    {
        private Lazy<Dictionary<string, string[]>> _dependencies = null;
        public Dictionary<string, string[]> Dependencies => _dependencies.Value;

        public abstract string RulesetCategory { get; }
        public abstract string OntologyDefinition { get; }
        public abstract string RulesetName { get; }
        public abstract Dictionary<string, IRFEARRule> Rules { get; }

        public FEARRuleSetBase()
        {
            _dependencies = new Lazy<Dictionary<string, string[]>>(BuildDependencies);
        }

        private Dictionary<string, string[]> BuildDependencies()
        {
            return null;
        }

        public string ConstructQueryFor(string sparqlConstructQuery)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var ontology in OntologyNamespaceImports)
            {
                sb.AppendLine($"PREFIX {ontology.Key}: <{ontology.Value}>");
            }

            foreach (var ns in NamespacePrefixes)
            {
                sb.AppendLine($"PREFIX {ns.Key}: <{ns.Value}>");
            }

            sb.AppendLine(sparqlConstructQuery);
            return sb.ToString();
        }
    }
}
