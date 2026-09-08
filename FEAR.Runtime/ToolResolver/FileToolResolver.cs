using FEAR.Domain.Infrastructure;
using FEAR.Domain.Model;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace FEAR.Runtime.ToolResolver
{
    /// <summary>
    /// Resolves, adds, and updates tool configurations from a JSON file for use in the FEAR pipeline.
    ///
    /// <para>
    /// <b>FileToolResolver</b> is responsible for managing the configuration of external tools (forensic, analysis, or transformation tools)
    /// that may be invoked as part of artifact or collector (CFEAR) execution. These tools are described in a JSON file (default: "tools.json")
    /// and can be dynamically added, updated, or resolved at runtime.
    /// </para>
    /// <para>
    /// In the context of graph codification, this resolver enables the FEAR system to discover and configure tools that process
    /// data queued for codification—whether that data is an artifact directly or the result of a collector script. This supports
    /// flexible, extensible, and reproducible tool-driven workflows across Blazor, WebAssembly, and Razor Pages projects.
    /// </para>
    /// </summary>
    public class FileToolResolver : IToolResolver
    {
        /// <summary>
        /// Builder for registering <see cref="FileToolResolver"/> in a DI context.
        /// </summary>
        public class Builder : IBuilderForContext
        {
            public void Build(HostApplicationBuilder _contextBuilder)
            {
                _contextBuilder.Services.TryAddTransient<IToolResolver>(
                    (ctx) => new FileToolResolver("tools.json")
                );
            }
        }

        /// <summary>
        /// Lazily loads the list of tool options from the JSON configuration file.
        /// </summary>
        protected Lazy<IList<ToolOptions>> _toolList;

        /// <summary>
        /// Gets the list of tool options currently loaded from the configuration file.
        /// </summary>
        public IList<ToolOptions> Tools => _toolList.Value;

        /// <summary>
        /// Gets or sets the path to the tool options JSON file.
        /// </summary>
        protected string OptionFile { get; set; } = "tools.json";

        /// <summary>
        /// Initializes a new instance of the <see cref="FileToolResolver"/> class using the default configuration file.
        /// </summary>
        public FileToolResolver()
        {
            _toolList = new Lazy<IList<ToolOptions>>(() =>
            {
                var tools = new List<ToolOptions>();
                if (File.Exists(OptionFile))
                {
                    var json = File.ReadAllText(OptionFile);
                    tools = JsonConvert.DeserializeObject<List<ToolOptions>>(json);
                }
                else
                {
                    File.WriteAllText(OptionFile, JsonConvert.SerializeObject(new List<ToolOptions>()));
                }

                return tools;
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileToolResolver"/> class with a specified configuration file.
        /// </summary>
        /// <param name="optionFile">The path to the tool options JSON file.</param>
        public FileToolResolver(string optionFile) : this()
        {
            OptionFile = optionFile;
        }

        /// <summary>
        /// Saves the current tool options to the configuration file.
        /// </summary>
        protected void SaveToolOptions()
        {
            File.WriteAllText(OptionFile, JsonConvert.SerializeObject(Tools));
        }

        /// <summary>
        /// Adds a new tool configuration if it does not already exist.
        /// </summary>
        /// <param name="toolOptions">The tool options to add.</param>
        public void AddTool(ToolOptions toolOptions)
        {
            if (!Tools.Any(t => t.ToolName == toolOptions.ToolName))
                Tools.Add(toolOptions);

            SaveToolOptions();
        }

        /// <summary>
        /// Resolves and retrieves the configuration options for a tool by its name.
        /// </summary>
        /// <param name="toolName">The name of the tool to resolve.</param>
        /// <returns>The <see cref="ToolOptions"/> for the specified tool, or null if not found.</returns>
        public ToolOptions? ResolveToolName(string toolName)
        {
            return Tools.FirstOrDefault(t => t.ToolName == toolName);
        }

        /// <summary>
        /// Updates an existing tool configuration in the resolver.
        /// </summary>
        /// <param name="toolOptions">The tool options to update.</param>
        public void UpdateTool(ToolOptions toolOptions)
        {
            var tool = Tools.FirstOrDefault(t => t.ToolName == toolOptions.ToolName);
            if (tool != null)
            {
                tool.ToolDirectory = toolOptions.ToolDirectory;
                tool.ToolExecutable = toolOptions.ToolExecutable;
                tool.ToolArguments = toolOptions.ToolArguments;
                tool.InputPlaceholder = toolOptions.InputPlaceholder;
                tool.RequiresOutputDirectory = toolOptions.RequiresOutputDirectory;
                tool.OutputPlaceholder = toolOptions.OutputPlaceholder;
                tool.IsFileOutput = toolOptions.IsFileOutput;
                tool.FileOuputName = toolOptions.FileOuputName;

                SaveToolOptions();
            }
        }

        /// <summary>
        /// Adds a new tool configuration or updates an existing one if it already exists.
        /// </summary>
        /// <param name="toolOptions">The tool options to add or update.</param>
        public void AddOrUpdateTool(ToolOptions toolOptions)
        {
            var tool = Tools.FirstOrDefault(t => t.ToolName == toolOptions.ToolName);
            if (tool != null)
            {
                UpdateTool(toolOptions);
            }
            else
            {
                AddTool(toolOptions);
            }
        }
    }
}
