using FEAR.Domain.KnowledgeGraph.IRI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FEAR.Domain.KnowledgeGraph.GraphManager
{
    /// <summary>
    /// Configuration settings for the Graph Manager, including IRI generation and ontology graph options.
    /// Supports binding from configuration and dependency injection of IRI generator factories.
    /// </summary>
    public class GraphManagerConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphManagerConfiguration"/> class.
        /// Binds configuration values from the "Configuration:GraphManager" section and
        /// retrieves the IRI generator factory from the service provider.
        /// </summary>
        /// <param name="provider">The service provider for dependency injection.</param>
        /// <param name="config">The application configuration.</param>
        public GraphManagerConfiguration(IServiceProvider provider, IConfiguration config)
        {
            if(config != null)
                config.GetSection("Configuration:GraphManager").Bind(this);
            
            IRIGeneratorFunc = provider.GetService<Func<string, IIRIGenerator>>();
        }

        /// <summary>
        /// Gets or sets the factory function for creating <see cref="IIRIGenerator"/> instances.
        /// The function takes a string key and returns an IRI generator.
        /// </summary>
        public Func<string, IIRIGenerator> IRIGeneratorFunc { get; set; }

        /// <summary>
        /// Gets or sets the folder path where ontology files are stored.
        /// </summary>
        public string OntologyGraphFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the ontology should be included in the materialized graph.
        /// </summary>
        public bool IncludeOntologyInMaterialisedGraph { get; set; }
    }
}
