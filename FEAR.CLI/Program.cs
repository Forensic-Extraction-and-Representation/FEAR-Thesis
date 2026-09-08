using FEAR.CLI.Arguments;
using FEAR.Domain.Arguments;
using FEAR.Runtime.Domain;
using FEAR.Runtime.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Parsing;

namespace FEAR.CLI
{
    internal class Program
    {
        [STAThreadAttribute]
        public static int Main(string[] args)
        {
            if (!args.Contains("--accept-experimental-terms"))
            {
                // Output a disclaimer to the user stating that it is provided without warranty and is experimental and ask them to enter "YES" to continue.
                Console.WriteLine("This software is provided without warranty and is experimental.");
                Console.WriteLine("This software is not designed to be used in a production environment.");
                Console.WriteLine("Do you wish to continue? (YES/NO)");

                var response = Console.ReadLine();
                if (response != "YES")
                {
                    Console.WriteLine("Please enter the YES or NO, in full, and capitalized to continue.");
                    response = Console.ReadLine();

                    if (response != "YES")
                    {
                        Console.WriteLine("Exiting...");
                        return 0;
                    }
                }
            }

            var configurationFileOption = new Option<string>(
                new string[] { "-c", "--configuration-file" },
                getDefaultValue: () => "configuration.json",
                description: "Configuration file");

            var scriptDirectoryOption = new Option<string>(
                new string[] { "-sd", "--script-directory" },
                getDefaultValue: () => "Source/",
                description: "Directory of scripts to be executed");

            var preCompiledDirectoryOption = new Option<string>(
                new string[] { "-pd", "--precompiled-directory" },
                getDefaultValue: () => "PreCompiled/",
                description: "Directory of precompiled scripts to be executed");

            var sourceCompilationOption = new Option<SourceCompilationEnum>(
                new string[] { "-sc", "--source-compilation" },
                getDefaultValue: () => SourceCompilationEnum.CompileScripts,
                description: "Source compilation (Source or PreCompiled)");

            var artifactsDirectoryOption = new Option<string>(
                new string[] { "-ad", "--artifacts-directory" },
                getDefaultValue: () => "artifacts",
                description: "Directory of artifacts of data to be processed. If you specify a full file path, that file will be used.");

            var inputFormatOption = new Option<string>(
                new string[] { "-if", "--input-format" },
                getDefaultValue: () => "JSON",
                description: "Data format (CSV, JSON, or XML)");

            inputFormatOption.AddValidator(option =>
            {
                if (option.Tokens.Count > 0)
                {
                    var format = option.Tokens[0].Value.ToLower();
                    if (format != "csv" && format != "json" && format != "xml")
                    {
                        option.ErrorMessage = $"Invalid format: {format}";
                    }
                }
            });

            var serializationFormatOption = new Option<string>(
                new string[] { "-sf", "--serialization-format", "--serialisation-format" },
                getDefaultValue: () => "TTL",
                description: "Output format (TTl, JSON-LD, or XML)");

            serializationFormatOption.AddValidator(option =>
            {
                if (option.Tokens.Count > 0)
                {
                    var format = option.Tokens[0].Value.ToLower();
                    if (format != "csv" && format != "json" && format != "xml")
                    {
                        option.ErrorMessage = $"Invalid format: {format}";
                    }
                }
            });

            var testCompileOption = new Option<bool?>(
                new string[] { "-T", "--test-compile" },
                description: "Test compile of scripts");

            var ontologyOutputFormatOption = new Option<string>(
                new string[] { "-ot", "--ontology-format" },
                getDefaultValue: () => "TTL",
                description: "Ontology output format (TTl, JSON-LD, or XML)");

            var acceptExperimentalTerms = new Option<bool?>(new string[] { "--accept-experimental-terms" });

            var outputFileNameOption = new Option<string>(
                new string[] { "-of", "--output-file" },
                getDefaultValue: () => @$"graph.ttl",
                description: "Output file name");

            var consoleOutputOption = new Option<bool?>(
                new string[] { "-co", "--console-output" },
                description: "Display the serialized RDF representation of the graph on the console.");

            var visualizeOption = new Option<bool?>(
                new string[] { "-v", "--visualize", "--visualise" },
                description: "Display a window for visualizing the graph using the MSAGL library.");

            var statisticsOption = new Option<bool?>(
                new string[] { "-s", "--statistics" },
                description: "Display statistics of the generated graph at the end of execution.");

            var displayTranspileOption = new Option<string[]>(
                new string[] { "-dt", "--display-transpile" },
                getDefaultValue: () => new string[] { },
                description: "Display transpiled code for a given Script file (.gfear extension is not required)"
            )
            { AllowMultipleArgumentsPerToken = true };

            var typeMatchingOption = new Option<TypeMatchingEnum>(
                new string[] { "-tm", "--type-matching" },
                getDefaultValue: () => TypeMatchingEnum.PropertyFallback,
                description: "Strict type matching");

            var namespaceOption = new Option<string>(
                new string[] { "-ns", "--namespace" },
                getDefaultValue: () => "http://fear.graph/",
                description: "Namespace of the graph");

            var namespaceAbbreviationOption = new Option<string>(
                new string[] { "-na", "--namespace-abbreviation" },
                getDefaultValue: () => "fear",
                description: "Namespace abbreviation of the graph");

            var webserviceOption = new Option<string>(
                new string[] { "-ws", "--webservice" },
                getDefaultValue: () => "",
                description: "The URL of the FEAR hosted web service to use");

            var rootCommand = new RootCommand("FEAR Command Line Interface")
            {
                configurationFileOption,
                scriptDirectoryOption,
                preCompiledDirectoryOption,
                artifactsDirectoryOption,
                sourceCompilationOption,
                inputFormatOption,
                serializationFormatOption,
                outputFileNameOption,
                consoleOutputOption,
                testCompileOption,
                namespaceOption,
                namespaceAbbreviationOption,
                webserviceOption,
                typeMatchingOption,
                displayTranspileOption,
                visualizeOption,
                statisticsOption,
                ontologyOutputFormatOption,
                acceptExperimentalTerms
            };

            rootCommand.SetHandler((invocationContext =>
            {
                if (invocationContext.ParseResult.Errors.Count > 0)
                {
                    invocationContext.HelpBuilder.Write(rootCommand, Console.Out);
                    return;
                }

                FearCliArguments cliArguments = new FearCliArguments
                {
                    WorkingDirectory = new Parameter<string>("."),
                    ConfigurationFile = new Parameter<string>(invocationContext.ParseResult.GetValueForOption(configurationFileOption)),
                    DisplayTranspileOption = new Parameter<string[]>(invocationContext.ParseResult.GetValueForOption(displayTranspileOption)),
                    ScriptDirectory = new Parameter<string>(invocationContext.ParseResult.GetValueForOption(scriptDirectoryOption)),
                    ArtifactsDirectory = new Parameter<string>(invocationContext.ParseResult.GetValueForOption(artifactsDirectoryOption)),
                    InputFormatOption = new Parameter<string>(invocationContext.ParseResult.GetValueForOption(inputFormatOption)),
                    OutputSerializationOption = new Parameter<string>(invocationContext.ParseResult.GetValueForOption(serializationFormatOption)),
                    OutputFileOption = new Parameter<string>(invocationContext.ParseResult.GetValueForOption(outputFileNameOption)),
                    ConsoleOutputOption = new Parameter<bool>(invocationContext.ParseResult.GetValueForOption(consoleOutputOption) ?? false),
                    TestCompileOption = new Parameter<bool>(invocationContext.ParseResult.GetValueForOption(testCompileOption) ?? false),
                    TypeMatchingOption = new Parameter<TypeMatchingEnum>(invocationContext.ParseResult.GetValueForOption(typeMatchingOption)),
                    VisualizeOption = new Parameter<bool>(invocationContext.ParseResult.GetValueForOption(visualizeOption) ?? false),
                    PreCompiledDirectory = new Parameter<string>(invocationContext.ParseResult.GetValueForOption(preCompiledDirectoryOption)),
                    LogsDirectory = new Parameter<string>("logs"),
                    PackageDirectory = new Parameter<string>("packages"),
                    SourceCompilationOption = new Parameter<SourceCompilationEnum>(invocationContext.ParseResult.GetValueForOption(sourceCompilationOption)),
                    NamespaceOption = new Parameter<string>(invocationContext.ParseResult.GetValueForOption(namespaceOption)),
                    NamespaceAbbrevOption = new Parameter<string>(invocationContext.ParseResult.GetValueForOption(namespaceAbbreviationOption)),
                    WebServiceAddressOption = new Parameter<string>(invocationContext.ParseResult.GetValueForOption(webserviceOption)),
                    OntologyOutputFormatOption = new Parameter<string>(invocationContext.ParseResult.GetValueForOption(ontologyOutputFormatOption)),
                    StatisticsOption = new Parameter<bool>(invocationContext.ParseResult.GetValueForOption(statisticsOption) ?? false)
                };

                FearCliArguments args = cliArguments.Copy();
                FearExecutionOptions<FearCliArguments> fearCliOptions = new FearCliOptions(args);
                FEARCompilerOptions compilerOptions = FEARCompilerOptions.DefaultCompilerOptions(fearCliOptions, (result) => PostCompileAction(fearCliOptions.TypedArguments.DisplayTranspileOption.Value, result));
                HostApplicationBuilder builder = FearExecutionContext<FearCliArguments>.DefaultBuilder<FearCliExecutionContext>(fearCliOptions, compilerOptions);

                var container = builder.Build();
                container.StartAsync();

                FearCliExecutionContext context = container.Services.GetService<FearCliExecutionContext>();
                context.Execute();

                if (fearCliOptions.TypedArguments.StatisticsOption.Value)
                {
                    var totalTriples = context.GraphManager.MaterializedGraph.Graph.Triples.Count();
                    var totalSubjects = context.GraphManager.MaterializedGraph.Graph.Triples.Select(t => t.Subject).Distinct().Count();
                    var totalPredicates = context.GraphManager.MaterializedGraph.Graph.Triples.Select(t => t.Predicate).Distinct().Count();
                    var totalObjects = context.GraphManager.MaterializedGraph.Graph.Triples.Select(t => t.Object).Distinct().Count();

                    //output the totals in json format
                    Console.WriteLine($"{{\"TotalTriples\":{totalTriples},\"TotalSubjects\":{totalSubjects},\"TotalPredicates\":{totalPredicates},\"TotalObjects\":{totalObjects}}}");
                }
            }));

            // Parse the command line arguments
            var parseResult = rootCommand.Parse(args);
            return parseResult.Invoke();
        }

        private static void PostCompileAction(string[] displayFileNames, FEARCompilationResult result)
        {
            var filename = result.CompilationRequest.FileName;
            if (displayFileNames.Length > 0 && displayFileNames.Any(t => filename.Contains(t, StringComparison.InvariantCultureIgnoreCase)))
            {
                Console.WriteLine($"Transpiled code for {filename}");
                Console.WriteLine(result.SourceCode);
            }
        }
    }
}