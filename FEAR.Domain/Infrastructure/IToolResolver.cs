using FEAR.Domain.Model;

namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Defines methods for resolving, adding, and updating tool configurations.
    /// </summary>
    public interface IToolResolver
    {
        /// <summary>
        /// Resolves and retrieves the configuration options for a tool by its name.
        /// </summary>
        /// <param name="toolName">The name of the tool to resolve.</param>
        /// <returns>The <see cref="ToolOptions"/> for the specified tool, or null if not found.</returns>
        ToolOptions ResolveToolName(string toolName);

        /// <summary>
        /// Adds a new tool configuration to the resolver.
        /// </summary>
        /// <param name="toolOptions">The tool options to add.</param>
        void AddTool(ToolOptions toolOptions);

        /// <summary>
        /// Updates an existing tool configuration in the resolver.
        /// </summary>
        /// <param name="toolOptions">The tool options to update.</param>
        void UpdateTool(ToolOptions toolOptions);

        /// <summary>
        /// Adds a new tool configuration or updates an existing one if it already exists.
        /// </summary>
        /// <param name="toolOptions">The tool options to add or update.</param>
        void AddOrUpdateTool(ToolOptions toolOptions);
    }
}
