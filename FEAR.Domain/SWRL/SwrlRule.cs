using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FEAR.Domain.SWRL
{
    /// <summary>
    /// Represents a SWRL rule, consisting of an antecedent (body) and a consequent (head).
    /// Each part is a list of <see cref="Atom"/> objects, which are the building blocks of SWRL rules.
    /// The antecedent contains the conditions (if), and the consequent contains the conclusions (then).
    /// </summary>
    public class SwrlRule
    {
        /// <summary>
        /// Gets or sets the list of atoms that form the antecedent (body) of the rule.
        /// These atoms represent the conditions that must be satisfied for the rule to fire.
        /// </summary>
        public List<Atom> Antecedent { get; set; } = new List<Atom>();

        /// <summary>
        /// Gets or sets the list of atoms that form the consequent (head) of the rule.
        /// These atoms represent the conclusions or actions that result when the antecedent is satisfied.
        /// </summary>
        public List<Atom> Consequent { get; set; } = new List<Atom>();
    }
}
