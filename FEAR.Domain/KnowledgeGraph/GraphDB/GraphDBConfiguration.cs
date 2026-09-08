using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FEAR.Domain.KnowledgeGraph.GraphDB
{
    /// <summary>
    /// Represents configuration settings for connecting to a graph database.
    /// Supports binding from an <see cref="IConfiguration"/> section named "Connections:GraphDBConnection".
    /// </summary>
    public abstract class GraphDBConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDBConfiguration"/> class
        /// and binds its properties from the specified configuration section.
        /// </summary>
        /// <param name="configuration">The application configuration to bind from.</param>
        public GraphDBConfiguration(IConfiguration configuration)
        {
            configuration.GetSection("Connections:GraphDBConnection").Bind(this);
        }

        /// <summary>
        /// Gets or sets the endpoint URL of the graph database.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the username for authentication (if required).
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password for authentication (if required).
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether authentication is required.
        /// </summary>
        public bool Authentication { get; set; }

        /// <summary>
        /// Gets or sets the URI of the graph to use (optional).
        /// </summary>
        public string? GraphUri { get; set; }
    }
}
