using FEAR.Runtime.Compiler;
using FEAR.Runtime.CSharpTranspile;
using FEAR.Runtime.Domain;
using FEAR.Runtime.GFEAR;
using FEAR.Runtime.RFEAR;
using FEAR.Runtime.TranspilerHelpers;
using FEAR.Runtime.TranspilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using VDS.RDF.Ontology;

namespace FEAR.RFEAR.Transpiler
{
    public partial class CSharpTranspiler : RFEARParserBaseVisitor<object>
    {
        private AssemblyManager _assemblyManager = new AssemblyManager();
        private List<FEARErrorBase> _transpileErrors = new List<FEARErrorBase>();

        public TranspileContext Context { get; private set; }

        public MemoryStream Transpile(FEARRuleSet[] rulsetFiles, FEARCompilationRequest options, TranspileContext ctx)
        {
            Context = ctx;
            List<SyntaxTree> trees = new List<SyntaxTree>();
            foreach (var ruleset in rulsetFiles)
            {
                trees.Add(CompileToSyntaxTree(ruleset, options));
            }

            return CSharpTranspileActions.CompileSyntaxTrees(new CSharpTranspileActions.SyntaxTreeCompileContext()
            {
                AssemblyManager = _assemblyManager,
                ParsedSyntaxTrees = trees.ToArray()
            }, options.TelemetrySignalService);
        }

        public MemoryStream Transpile(FEARRuleSet rulsetFiles, FEARCompilationRequest options, TranspileContext ctx)
        {
            var Context = ctx;
            var parsedSyntaxTree = CompileToSyntaxTree(rulsetFiles, options);
            return CSharpTranspileActions.CompileSyntaxTrees(new CSharpTranspileActions.SyntaxTreeCompileContext()
            {
                AssemblyManager = _assemblyManager,
                ParsedSyntaxTrees = new SyntaxTree[] { parsedSyntaxTree }
            }, options.TelemetrySignalService);
        }

        private string TranspileRuleToCSharp(RuleDefinition rule)
        {
            string rn = rule.RuleName.Replace("\"", "");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"public class Rule_{rule.RuleType}_{rn} : FEARRuleBase, IRFEARRule");
            sb.AppendLine("{");
            sb.AppendLine($"    public Rule_{rule.RuleType}_{rn}() : base() {{ }}");
            sb.AppendLine($"    public override string {nameof(FEARRuleBase.RuleName)} => \"{rule.RuleName}\";");
            sb.AppendLine($"    public override string {nameof(FEARRuleBase.RuleType)} => \"{rule.RuleType}\";");
            sb.AppendLine($"    public override string {nameof(FEARRuleBase.SparqlInsertQuery)} => @\"{rule.SparqlInsertQuery}\";");
            sb.AppendLine($"    public override string {nameof(FEARRuleBase.SparqlConstructQuery)} => @\"{rule.SparqlConstructQuery}\";");
            sb.AppendLine($"    public override string {nameof(FEARRuleBase.SwrlRule)} => @\"{rule.SwrlRule.Replace("\"", "\"\"")}\";");

            if (rule.DependsOn != null && rule.DependsOn.Count > 0)
            {
                sb.AppendLine($"    public override List<string> DependsOn => new List<string> {{ " +
                    $"{string.Join(",\n", rule.DependsOn.Select(d => $"\"{d}\""))}" +
                    $" }};");
            }
            else
            {
                sb.AppendLine("    public override List<string> DependsOn => new List<string>();");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private SyntaxTree CompileToSyntaxTree(FEARRuleSet rulsetFiles, FEARCompilationRequest options)
        {
            RuleSetDefinitionBlock rsd = CompileSwrlRules(rulsetFiles, options);
            StringBuilder sourceCodeBuilder = new StringBuilder();
            // From here we need to produce the library code that is what is executable by FEAR Host
            // Need to consider checks for dependencies and only run ones which all dependencies are present for
            // It is the responsibility of the host to run these periodically.
            List<string> ruleSourceCode = new List<string>();
            OntologyGenerator ontologyGenerator = new OntologyGenerator();
            OntologyGraph ontologyDefinition = ontologyGenerator.GenerateOntology(rsd, options);

            foreach (var rule in rsd.RuleDefinitions)
            {
                ruleSourceCode.Add(TranspileRuleToCSharp(rule));
            }

            var rulesetNamespace = rsd.RuleSetCategory.Replace("\"", "").Replace("/", ".");
            var className = rsd.RuleSetName.Replace("\"", "").Replace("+", "");
            _assemblyManager.AssemblyName = rulesetNamespace + ".dll";

            sourceCodeBuilder.AppendLine($"""
                using System;
                using System.IO;
                using System.Collections.Generic;

                using FEAR.Domain.RuleSet;
                using FEAR.Runtime.RFEAR;
                namespace FEAR.Rulesets.Compiled.{rulesetNamespace}
                """);

            var ttlWriter = new VDS.RDF.Writing.CompressingTurtleWriter();
            ttlWriter.CompressionLevel = 0;
            ttlWriter.PrettyPrintMode = false;
            ttlWriter.HighSpeedModePermitted = true;
            StringBuilder ttl = new StringBuilder();
            using (StringWriter sw = new StringWriter(ttl))
            {
                ttlWriter.Save(ontologyDefinition, sw);
            }
            var ttlOntologyDef = ttl.ToString();

            sourceCodeBuilder.AppendLine("{");
            sourceCodeBuilder.AppendLine($@"[{nameof(FEARRuleSet)}({rsd.RuleSetCategory}, {rsd.RuleSetName})]");
            sourceCodeBuilder.AppendLine($"    public class RulSet_Definition_{className} : {nameof(FEARRuleSetBase)}");
            sourceCodeBuilder.AppendLine("{");
            sourceCodeBuilder.AppendLine($"        public override string {nameof(FEARRuleSetBase.OntologyDefinition)} => @\"");
            sourceCodeBuilder.AppendLine($"        {ttlOntologyDef}");
            sourceCodeBuilder.AppendLine($"        \";");

            sourceCodeBuilder.AppendLine($"        public override string {nameof(FEARRuleSetBase.RulesetCategory)} => {rsd.RuleSetCategory};");
            sourceCodeBuilder.AppendLine($"        public override string {nameof(FEARRuleSetBase.RulesetName)} => {rsd.RuleSetName};");
            // add the ontology and import statements
            sourceCodeBuilder.AppendLine($"        public override Dictionary<string, IRFEARRule> {nameof(FEARRuleSetBase.Rules)} => new Dictionary<string, IRFEARRule>() {{");
            foreach (var rule in rsd.RuleDefinitions)
            {
                sourceCodeBuilder.AppendLine($"            {{ \"{rule.RuleName}\", new Rule_{rule.RuleType}_{rule.RuleName}() }},");
            }
            sourceCodeBuilder.AppendLine("        };");

            sourceCodeBuilder.AppendLine($"        public override Dictionary<string, Uri> {nameof(FEARLanguageBase.OntologyNamespaceImports)} => new Dictionary<string, Uri>() {{");
            foreach (var ontology in rsd.OntologyStatements)
            {
                sourceCodeBuilder.AppendLine($"            {{ \"{ontology.Prefix}\", new Uri(\"{ontology.Uri}\") }},");
            }
            sourceCodeBuilder.AppendLine("        };");

            // Include and prefix statements
            var iStmt = rsd.IncludeStatements;
            var includes = String.Join(Environment.NewLine, iStmt.Select((a) => $@"{nameof(FEARLanguageBase.AddDefaultNamespace)}(""{a}"");"));
            var pStmt = rsd.PrefixStatements;
            var prefixes = String.Join(Environment.NewLine, pStmt.Select((a) => $@"{nameof(FEARLanguageBase.AddNamespacePrefix)}(""{a.Prefix}"", new Uri(""{a.Namespace}""));"));

            sourceCodeBuilder.AppendLine("        public RulSet_Definition_" + className + "() : base() {");
            sourceCodeBuilder.AppendLine($@"                {includes}");
            sourceCodeBuilder.AppendLine($@"                {prefixes}");
            sourceCodeBuilder.AppendLine("        }");

            sourceCodeBuilder.AppendJoin(Environment.NewLine, ruleSourceCode);

            sourceCodeBuilder.AppendLine("    }");
            sourceCodeBuilder.AppendLine("}");

            var combinedCode = sourceCodeBuilder.ToString();

            // encode source code
            string sourceCode = SourceText.From(combinedCode, Encoding.UTF8).ToString();

            // Write transpiled source if enabled
            TranspiledSourceWriter.WriteIfEnabled("rfear", rulsetFiles.SourceFileName, sourceCode, options);

            FEARCompilationResult fcr = new FEARCompilationResult(sourceCode, new FEARCompilationRequest(options.CompilerOptions, options.ExecutionOptions, options.OntologyStore, options.TelemetrySignalService, rulsetFiles.SourceFileName, options.OutFile));
            options.CompilerOptions.PostCompileAction(fcr);

            return CSharpTranspileActions.Parse(sourceCode, "", CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11));
        }

        private RuleSetDefinitionBlock CompileSwrlRules(FEARRuleSet FEARProgram, FEARCompilationRequest options)
        {
            CompilerTelemetry.SendCompilationTelemetrySignal($"Transpiling File - {options.FileName} {FEARProgram.SourceFileName}", options.TelemetrySignalService);
            var rsd = (RuleSetDefinitionBlock)VisitRulesetDefinitionBlock(FEARProgram.RFEARCtxRoot.rulesetDefinitionBlock());
            rsd.Filename = FEARProgram.SourceFileName;

            foreach (var rb in FEARProgram.RFEARCtxRoot.ruleBlock())
            {
                var rbb = ParseRuleBlock(rb, rsd);

                FEARCompilationResult fcr = new FEARCompilationResult(rbb.SparqlInsertQuery, new FEARCompilationRequest(options.CompilerOptions, options.ExecutionOptions, options.OntologyStore, options.TelemetrySignalService, FEARProgram.SourceFileName, options.OutFile));
                options.CompilerOptions.PostCompileAction(fcr);
            }

            if (_transpileErrors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Transpile Errors in {FEARProgram.SourceFileName}:");

                foreach (var error in _transpileErrors)
                {
                    sb.AppendLine(error.ToString());
                }

                CompilerTelemetry.SendCompilationTelemetrySignal($"Transpile Errors in {FEARProgram.SourceFileName}:\n{sb.ToString()}", options.TelemetrySignalService);
            }

            return rsd;
        }
    }
}
