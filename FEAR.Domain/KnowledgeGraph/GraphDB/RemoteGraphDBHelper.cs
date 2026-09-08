using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace FEAR.Domain.KnowledgeGraph.GraphDB
{
    /// <summary>
    /// Helper class for managing remote graph database connections and operations.
    /// Provides methods for saving and writing RDF graphs using a generic connection type.
    /// </summary>
    /// <typeparam name="T">A type implementing <see cref="IGraphDBConnection"/>.</typeparam>
    public class RemoteGraphDBHelper<T>
        where T : IGraphDBConnection
    {
        // Lazily-initialized connection to the remote graph database.
        private Lazy<T> graphDbConnection = null;

        /// <summary>
        /// Gets the underlying remote graph database connection.
        /// </summary>
        T GraphDbConnection => graphDbConnection.Value;

        /// <summary>
        /// Gets the factory used to construct the remote graph database connection.
        /// </summary>
        public GraphDBConnectionFactory<T> ConnectionFactory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteGraphDBHelper{T}"/> class
        /// with the specified connection factory.
        /// </summary>
        /// <param name="connectionFactory">The factory for creating the graph database connection.</param>
        public RemoteGraphDBHelper(GraphDBConnectionFactory<T> connectionFactory)
        {
            ConnectionFactory = connectionFactory;
            graphDbConnection = new Lazy<T>(() => ConnectionFactory.ConstructConnection());
        }

        /// <summary>
        /// Writes the provided RDF graph to the specified stream in Turtle format.
        /// If no stream is provided, writes to standard output.
        /// </summary>
        /// <param name="graph">The RDF graph to write.</param>
        /// <param name="stream">The output stream (optional).</param>
        public static void WriteGraph(IGraph graph, Stream stream = null)
        {
            stream = stream ?? Console.OpenStandardOutput();

            IRdfWriter writer = new RdfXmlWriter();
            ITripleFormatter formatter = new VDS.RDF.Writing.Formatting.TurtleFormatter();
            TurtleW3CFormatter formatter2 = new TurtleW3CFormatter();
            CompressingTurtleWriter writer2 = new CompressingTurtleWriter();
            foreach (var triple in graph.Triples)
            {
                // Console.WriteLine(formatter.Format(triple));
            }

            Console.WriteLine("==================================");
            Console.WriteLine("");
            writer2.Save(graph, new StreamWriter(stream));
            Console.WriteLine("");
            Console.WriteLine("==================================");

            Console.WriteLine("");
        }

        /// <summary>
        /// Saves the provided ontology graph to the remote store at the specified graph URI.
        /// Deletes the existing graph if supported, then uploads triples in batches.
        /// </summary>
        /// <param name="graph">The ontology graph to save.</param>
        /// <param name="graphUri">The URI of the graph in the remote store.</param>
        /// <exception cref="Exception">
        /// Thrown if the store does not support graph-level deletes or triple-level updates.
        /// </exception>
        public void SaveGraph(OntologyGraph graph, Uri graphUri)
        {
            if (GraphDbConnection.DeleteSupported)
            {
                var graphs = GraphDbConnection.ListGraphs();
                if (graphs.Contains(graphUri))
                {
                    // Delete the Graph from the Store
                    GraphDbConnection.DeleteGraph(graphUri);
                }
            }
            else
            {
                throw new Exception("Store does not support graph level deletes");
            }

            if (GraphDbConnection.UpdateSupported)
            {
                int batchSize = 20;
                int skipCount = 0;
                while (true)
                {
                    // Get the next batch of items
                    IEnumerable<Triple> batch = graph.Triples.Skip(skipCount).Take(batchSize);

                    // UpdateGraph takes enumerables of Triples to add/remove or null to indicate none
                    // Hence why we create a Triple array to pass in the Triple to be deleted
                    GraphDbConnection.UpdateGraph(graphUri, batch.ToArray(), null);

                    // Increment the skip count
                    skipCount += batchSize;

                    // Break the loop if there are no more items
                    if (batch.Count() < batchSize)
                        break;
                }
            }
            else
            {
                throw new Exception("Store does not support triple level updates");
            }
        }
    }
}
