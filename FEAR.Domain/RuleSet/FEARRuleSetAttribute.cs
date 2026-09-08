namespace FEAR.Domain.RuleSet
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FEARRuleSetAttribute : Attribute
    {
        public string RulesetName { get; }
        public string RulesetCategory { get; }
        public FEARRuleSetAttribute(string rulesetName, string rulesetCategory)
        {
            RulesetName = rulesetName;
            RulesetCategory = rulesetCategory;
        }
    }
}
