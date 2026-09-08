using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEAR.Domain.RuleSet
{
    public interface IFEARRuleSet : IFEARLanguageBase
    {
        string RulesetCategory { get; }
        string RulesetName { get; }

        Dictionary<string, IRFEARRule> Rules { get; }

        string ConstructQueryFor(string sparqlConstructQuery);
    }
}
