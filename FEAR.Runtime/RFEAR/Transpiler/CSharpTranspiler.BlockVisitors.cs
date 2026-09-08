using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using FEAR.Domain.SWRL;
using System.Text;
using VDS.RDF.Query;
using CSTH = FEAR.Runtime.Helpers.CSharpTranspilerHelpers;

namespace FEAR.RFEAR.Transpiler
{
    public partial class CSharpTranspiler
    {
        protected void WriteLine(StringBuilder sb, int tabs, string line)
        {
            sb.AppendLine(CSTH.Tab(tabs) + line);
        }

        public override object VisitClassAtom([NotNull] RFEARParser.ClassAtomContext context)
        {
            AtomVariable atomVariable = new AtomVariable
            {
                Name = context.variable().GetText(),
                Type = VariableTypeEnum.Variable
            };

            ClassAtom classAtom = new ClassAtom
            {
                Iri = context.iri().GetText(),
                Variables = new List<AtomVariable>() { atomVariable }
            };

            return classAtom;
        }

        public override object VisitPropertyAtom([NotNull] RFEARParser.PropertyAtomContext context)
        {
            AtomVariable subjectVariable = new AtomVariable
            {
                Name = context.subjectVariable.GetText(),
                Type = VariableTypeEnum.Variable
            };
            AtomVariable objectVariable = new AtomVariable
            {
                Name = context.objectVariable.GetText(),
                Type = VariableTypeEnum.Variable
            };
            PropertyAtom propertyAtom = new PropertyAtom
            {
                Iri = context.iri().GetText(),
                Variables = new List<AtomVariable> { subjectVariable, objectVariable }
            };
            return propertyAtom;
        }

        public override object VisitBuiltinAtom([NotNull] RFEARParser.BuiltinAtomContext context)
        {
            AtomVariable subjectVariable = new AtomVariable
            {
                Name = context.arg1.GetText(),
                Type = VariableTypeEnum.Variable
            };
            AtomVariable objectVariable = new AtomVariable
            {
                Name = context.arg2.GetText(),
                Type = VariableTypeEnum.Variable
            };
            BuiltInAtom builtInAtom = new BuiltInAtom
            {
                Iri = context.swrlbIri().GetText(),
                Variables = new List<AtomVariable> { subjectVariable, objectVariable }
            };
            return builtInAtom;
        }

        public override object VisitAtom([NotNull] RFEARParser.AtomContext context)
        {
            if (context.classAtom() != null)
            {
                return VisitClassAtom(context.classAtom());
            }
            else if (context.propertyAtom() != null)
            {
                return VisitPropertyAtom(context.propertyAtom());
            }
            else if (context.builtinAtom() != null)
            {
                return VisitBuiltinAtom(context.builtinAtom());
            }
            else
            {
                throw new NotSupportedException("Unknown atom type encountered.");
            }
        }

        public override object VisitSwrlAntecedent([NotNull] RFEARParser.SwrlAntecedentContext context)
        {
            List<Atom> atoms = new List<Atom>();
            foreach (var atom in context.atom())
            {
                atoms.Add((Atom)VisitAtom(atom));
            }
            return atoms;
        }

        public override object VisitSwrlConsequent([NotNull] RFEARParser.SwrlConsequentContext context)
        {
            List<Atom> atoms = new List<Atom>();
            foreach (var atom in context.atom())
            {
                atoms.Add((Atom)VisitAtom(atom));
            }
            return atoms;
        }

        public override object VisitSwrlRule([NotNull] RFEARParser.SwrlRuleContext context)
        {
            SwrlRule rule = new SwrlRule();
            rule.Antecedent = (List<Atom>)VisitSwrlAntecedent(context.swrlAntecedent());
            rule.Consequent = (List<Atom>)VisitSwrlConsequent(context.swrlConsequent());
            return rule;
        }

        public override object VisitRuleBlock([NotNull] RFEARParser.RuleBlockContext context)
        {
            var rd = new RuleDefinition
            {
                RuleName = context.ruleName.Text,
                RuleType = context.ruleType.Text,
                SwrlRule = context.swrlRule().GetText() ?? "",
                SparqlInsertQuery = "",
                SparqlConstructQuery = "",
            };

            SwrlRule rule = (SwrlRule)VisitSwrlRule(context.swrlRule());

            StringBuilder sparqlWhereClause = new StringBuilder();
            StringBuilder insertQuery = new StringBuilder();
            StringBuilder constructQuery = new StringBuilder();

            sparqlWhereClause.AppendLine("WHERE {");
            foreach (var atom in rule.Antecedent)
            {
                if (atom is BuiltInAtom)
                {
                    string filterCondition = $"FILTER({atom.ToSparqlString()})";
                    sparqlWhereClause.AppendLine(filterCondition);
                }
                else
                {
                    sparqlWhereClause.AppendLine(atom.ToSparqlString());
                }
            }

            sparqlWhereClause.AppendLine("FILTER(");
            sparqlWhereClause.AppendLine(
                String.Join(
                    "||\n",
                    rule.Consequent.Select(csq => $"NOT EXISTS {{ {csq.ToSparqlString().TrimEnd('.')}}} ")
                )
            );

            sparqlWhereClause.AppendLine(")");
            sparqlWhereClause.AppendLine("}");

            insertQuery.AppendLine($"# Rule: {rd.RuleName}");
            constructQuery.AppendLine($"# Rule: {rd.RuleName}");

            insertQuery.AppendLine("INSERT {");
            constructQuery.AppendLine("CONSTRUCT {");

            foreach (var atom in rule.Consequent)
            {
                insertQuery.AppendLine(atom.ToSparqlString());
                constructQuery.AppendLine(atom.ToSparqlString());
            }

            insertQuery.AppendLine("}");
            constructQuery.AppendLine("}");

            insertQuery.Append(sparqlWhereClause.ToString());
            constructQuery.Append(sparqlWhereClause.ToString());

            rd.SparqlInsertQuery = insertQuery.ToString();
            rd.SparqlConstructQuery = constructQuery.ToString();
            rd.ParsedRule = rule;
            return rd;
        }

        public RuleDefinition ParseRuleBlock([NotNull] RFEARParser.RuleBlockContext context, RuleSetDefinitionBlock ruleSet)
        {
            RuleDefinition ruleDef = (RuleDefinition)VisitRuleBlock(context);
            var dependsOnClauseBlockContext = context.dependsOnClauseBlock();
            if (dependsOnClauseBlockContext != null)
                ParseRuleDependencies(dependsOnClauseBlockContext, ruleDef);
            ruleSet.RuleDefinitions.Add(ruleDef);

            return ruleDef;
        }

        private void ParseRuleDependencies(RFEARParser.DependsOnClauseBlockContext dependsOnClauseBlockContext, [NotNull] RuleDefinition ruleDef)
        {
            foreach (var dClause in dependsOnClauseBlockContext.dependsOnClause())
            {
                ruleDef.DependsOn.Add(dClause.ruleName.Text);
            }
        }

        public override object VisitRulesetDefinitionBlock([NotNull] RFEARParser.RulesetDefinitionBlockContext context)
        {
            RuleSetDefinitionBlock ruleSet = new RuleSetDefinitionBlock();
            ruleSet.RuleSetName = context.rulesetDefinitionStatement().rulsetName.Text;
            ruleSet.RuleSetCategory = context.rulesetDefinitionStatement().rulesetCategory.Text;

            var iStmt = context.includeStatement();
            var pStmt = context.prefixStatement();

            foreach (var ontology in context.ontologyStatement())
            {
                RuleSetDefinitionBlock.OntologyStatement os = new RuleSetDefinitionBlock.OntologyStatement();
                os.Prefix = ontology.ontPrefix.Text.Trim(['\'', '"']);
                os.Uri = ontology.ontUrl.Text.Trim(['\'', '"']);

                ruleSet.OntologyStatements.Add(os);
            }

            foreach (var include in iStmt.swrlbOrIdentifier())
            {
                ruleSet.IncludeStatements.Add(include.GetText());
            }

            foreach (var prefix in pStmt)
            {
                RuleSetDefinitionBlock.PrefixStatement ps = new RuleSetDefinitionBlock.PrefixStatement
                {
                    Prefix = prefix.prefixLabel.Text,
                    Namespace = prefix.prefixUri.Text
                };
                ruleSet.PrefixStatements.Add(ps);
            }

            return ruleSet;
        }
    }
}
