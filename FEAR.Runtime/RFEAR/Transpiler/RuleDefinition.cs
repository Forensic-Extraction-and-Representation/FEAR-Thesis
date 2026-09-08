using FEAR.Domain.RuleSet;
using FEAR.Domain.SWRL;

namespace FEAR.RFEAR.Transpiler
{
    public class RuleDefinition : IRFEARRule
    {
        public string RuleName { get; set; }
        public string RuleType { get; set; }
        public string SparqlInsertQuery { get; set; }
        public string SparqlConstructQuery { get; set; }
        public string SwrlRule { get; set; }
        public List<string> DependsOn { get; set; } = new List<string>();
        public SwrlRule ParsedRule { get; internal set; }
    }
}
