using System.Configuration;
using System.Reflection.Metadata;
using FEAR.Domain.Arguments;

namespace FEAR.CLI.Arguments
{    public class FearCliArguments : FearArguments
    {
        public Parameter<string> ConfigurationFile { get; set; }
        public Parameter<string> ArtifactsDirectory { get; set; }
        public Parameter<string> InputFormatOption { get; set; }
        public Parameter<string> OutputFileOption { get; set; }
        public Parameter<string> OutputSerializationOption { get; set; }
        public Parameter<bool> VisualizeOption { get; set; }
        public Parameter<bool> ConsoleOutputOption { get; set; }
        public Parameter<bool> StatisticsOption { get; set; }
        public Parameter<string> WebServiceAddressOption { get; set; }

        public virtual string GetArtifactsDirectory()
        {
            return ArtifactsDirectory.IsSet && !string.IsNullOrEmpty(ArtifactsDirectory?.Value) ? Path.Combine(GetWorkingDirectory(), ArtifactsDirectory.Value) : Path.Combine(GetWorkingDirectory(), "Artifacts");
        }

        public FearCliArguments Copy()
        {
            FearCliArguments copy = new FearCliArguments()
            {

                PreCompiledDirectory = this.PreCompiledDirectory,
                SourceCompilationOption = this.SourceCompilationOption,
                ScriptDirectory = this.ScriptDirectory,
                ArtifactsDirectory = this.ArtifactsDirectory,
                InputFormatOption = this.InputFormatOption,
                OutputFileOption = this.OutputFileOption,
                OutputSerializationOption = this.OutputSerializationOption,
                TestCompileOption = this.TestCompileOption,
                TypeMatchingOption = this.TypeMatchingOption,
                DisplayTranspileOption = this.DisplayTranspileOption,
                VisualizeOption = this.VisualizeOption,
                ConsoleOutputOption = this.ConsoleOutputOption,
                ConfigurationFile = this.ConfigurationFile,
                NamespaceOption = this.NamespaceOption,
                NamespaceAbbrevOption = this.NamespaceAbbrevOption,
                StatisticsOption = this.StatisticsOption,
                WebServiceAddressOption = this.WebServiceAddressOption,
                SourceRepositoryPaths = this.SourceRepositoryPaths,
                PrecompiledLibraryPaths = this.PrecompiledLibraryPaths,
                WorkingDirectory = this.WorkingDirectory,
                OntologyOutputFormatOption = this.OntologyOutputFormatOption,
                RestrictTo = this.RestrictTo,
                LogsDirectory = this.LogsDirectory,
                OutputTranspiledSource = this.OutputTranspiledSource,
                PackageDirectory = this.PackageDirectory,
                PackagedSourcesPaths = this.PackagedSourcesPaths,
                TranspiledSourceOutputDirectory = this.TranspiledSourceOutputDirectory,
            };

            return copy;
        }
    }
}
