namespace FEAR.RFEAR.Transpiler
{
    public class RuleSetDefinitionBlock
    {
        public string RuleSetName { get; set; }
        public string RuleSetCategory { get; set; }
        public string Filename { get; set; }

        public List<OntologyStatement> OntologyStatements { get; set; } = new List<OntologyStatement>();
        public List<PrefixStatement> PrefixStatements { get; set; } = new List<PrefixStatement>();
        public List<string> IncludeStatements { get; set; } = new List<string>();
        public List<RuleDefinition> RuleDefinitions { get; set; } = new List<RuleDefinition>();

        public class OntologyStatement
        {
            public String Prefix { get; set; }
            public String Uri { get; set; }
        }

        public class PrefixStatement
        {
            public string Prefix { get; set; }
            public string Namespace { get; set; }
        }
    }
}
