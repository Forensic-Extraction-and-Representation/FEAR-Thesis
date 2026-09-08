using Microsoft.Extensions.Hosting;

namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Generic interface for types that need to participate in a builder-based configuration or initialization process.
    /// Implementations can be used to configure, extend, or modify a builder context (such as <see cref="HostApplicationBuilder"/>)
    /// in a modular and reusable way. This pattern is applicable to any scenario where a builder is used to construct
    /// or configure application components, services, or environments.
    /// </summary>
    public interface IBuilderForContext
    {
        /// <summary>
        /// Applies configuration or initialization logic to the provided builder context.
        /// </summary>
        /// <param name="_contextBuilder">The builder context to configure or extend (e.g., <see cref="HostApplicationBuilder"/>).</param>
        void Build(HostApplicationBuilder _contextBuilder);
    }
}
