using System.IO; // ensure Path is available
using FEAR.Domain.Arguments;
using Microsoft.Extensions.Configuration;

namespace FEAR.Host.Core.Services
{
    public class FearHostedArguments : FearArguments
    {
        private readonly IConfiguration _configuration;
        private readonly IConfiguration _systemConfiguration;
        private Lazy<string> _workingDirectory;
        public FearHostedArguments(IConfiguration caseConfiguration, IConfiguration systemConfiguration)
            : base()
        {
            _configuration = caseConfiguration;
            _systemConfiguration = systemConfiguration;
            this.WorkingDirectory = new Parameter<string>(caseConfiguration.GetValue<string>(nameof(FearArguments.WorkingDirectory)));
            this.SourceCompilationOption = new Parameter<SourceCompilationEnum>(caseConfiguration.GetValue<SourceCompilationEnum>(nameof(FearArguments.SourceCompilationOption), SourceCompilationEnum.Both));

            this.PreCompiledDirectory = new Parameter<string>(caseConfiguration.GetValue<string>(nameof(FearArguments.PreCompiledDirectory), "PreCompiled"));
            this.ScriptDirectory = new Parameter<string>(caseConfiguration.GetValue<string>(nameof(FearArguments.ScriptDirectory), "Source"));
            this.PackageDirectory = new Parameter<string>(caseConfiguration.GetValue<string>(nameof(FearArguments.PackageDirectory), "Packages"));
            this.LogsDirectory = new Parameter<string>(caseConfiguration.GetValue<string>(nameof(FearArguments.LogsDirectory), "Logs"));
            this.TranspiledSourceOutputDirectory = new Parameter<string>(caseConfiguration.GetValue<string>(nameof(FearArguments.TranspiledSourceOutputDirectory), "TranspiledSource"));

            this.TypeMatchingOption = new Parameter<TypeMatchingEnum>(caseConfiguration.GetValue<TypeMatchingEnum>(nameof(FearArguments.TypeMatchingOption), TypeMatchingEnum.Strict));

            this.NamespaceOption = new Parameter<string>(caseConfiguration.GetValue<string>(nameof(FearArguments.NamespaceOption)));
            this.NamespaceAbbrevOption = new Parameter<string>(caseConfiguration.GetValue<string>(nameof(FearArguments.NamespaceAbbrevOption)));

            this.OutputTranspiledSource = new Parameter<bool>(caseConfiguration.GetValue<bool>(nameof(FearArguments.OutputTranspiledSource), false));

            this._workingDirectory = new Lazy<string>(() =>
            {
                string workingDirectory = base.GetWorkingDirectory();
                string caseDirectory = _systemConfiguration.GetSection("SystemDefaults")?.GetValue<string>("CaseBaseDirectory") ?? "Cases";

                // if the working directory is not an absolute path, combine it with the case directory
                if (!Path.IsPathRooted(workingDirectory))
                {
                    workingDirectory = Path.Combine(caseDirectory, workingDirectory);
                }

                return workingDirectory;
            });
        }


        public override string GetWorkingDirectory()
        {
            return _workingDirectory.Value;
        }
    }
}
