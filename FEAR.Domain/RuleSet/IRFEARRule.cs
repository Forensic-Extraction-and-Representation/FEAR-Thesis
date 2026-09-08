namespace FEAR.Domain.RuleSet
{
    public interface IRFEARRule
    {
        public string RuleName { get; }
        public string RuleType { get; }
        public string SparqlInsertQuery { get; }
        public string SparqlConstructQuery { get; }
        public string SwrlRule { get; }
        public List<string> DependsOn { get; }
    }
}
