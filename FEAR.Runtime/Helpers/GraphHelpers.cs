using FEAR.Runtime.KnowledgeGraph;
using System.Text;
using VDS.RDF;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace FEAR.Runtime.Helpers
{
    /// <summary>
    /// Provides helper methods for writing and serializing RDF graphs in various formats.
    ///
    /// These helpers are used in the FEAR pipeline to output knowledge graphs that have been
    /// constructed or codified from queued data. The data processed and written by these helpers
    /// can be either:
    ///   - An artifact directly (raw evidence or input data)
    ///   - The result of a collector (CFEAR) script
    ///
    /// The output is typically used for inspection, debugging, or exporting the results of
    /// graph codification, regardless of whether the source was a direct artifact or a collector result.
    /// </summary>
    public static class GraphHelpers
    {
        public static string SerializeTriples(this Graph t)
        {
            TurtleFormatter formatter = new TurtleFormatter();
            StringBuilder sb = new StringBuilder();
            foreach (var triple in t.Triples)
            {
                sb.AppendLine(formatter.Format(triple));
            }

            return sb.ToString();
        }

        public static string GetGraphAsTurtle(IGraph graph)
        {
            var ms = new MemoryStream();
            GraphHelpers.WriteGraph("xml", graph, new List<Stream> { ms });
            ms.Position = 0;
            string ttl = new StreamReader(ms).ReadToEnd();

            return ttl;
        }

        /// <summary>
        /// Writes the provided RDF graph to the specified streams in Turtle format.
        /// If no streams are provided, writes to standard output.
        /// </summary>
        /// <param name="graph">The RDF graph to write.</param>
        /// <param name="streams">The output streams (optional).</param>
        public static void WriteGraph(IGraph graph, IEnumerable<Stream> streams = null)
        {
            streams = streams ?? new List<Stream>() { Console.OpenStandardOutput() };

            foreach (var stream in streams)
                WriteGraph(new CompressingTurtleWriter(), graph, stream);
            return;

            // The following code is unreachable, but left for reference on alternative writers/formatters.
            IRdfWriter writer = new RdfXmlWriter();
            ITripleFormatter formatter = new TurtleFormatter();
            TurtleW3CFormatter formatter2 = new TurtleW3CFormatter();
            CompressingTurtleWriter writer2 = new CompressingTurtleWriter();
            foreach (var triple in graph.Triples)
            {
                //Console.WriteLine(formatter.Format(triple));
            }

            Console.WriteLine("==================================");
            Console.WriteLine("");
            foreach (var stream in streams)
                writer2.Save(graph, new StreamWriter(stream));
            Console.WriteLine("");
            Console.WriteLine("==================================");

            Console.WriteLine("");
        }

        /// <summary>
        /// Writes the provided RDF graph to the specified streams using the given writer type.
        /// Supported writer types: "jsonld", "xml", or default (Turtle).
        /// </summary>
        /// <param name="writerType">The writer type ("jsonld", "xml", or default).</param>
        /// <param name="graph">The RDF graph to write.</param>
        /// <param name="streams">The output streams (optional).</param>
        public static void WriteGraph(string writerType, IGraph graph, IEnumerable<Stream> streams = null)
        {
            if(streams == null)
                streams = new List<Stream>() { Console.OpenStandardOutput() };

            IRdfWriter rdfWriter = new CompressingTurtleWriter();
            if(writerType.ToLower() == "jsonld")
                rdfWriter = new RdfJsonWriter();
            if (writerType.ToLower() == "xml")
                rdfWriter = new RdfXmlWriter();

            foreach (var stream in streams)
                WriteGraph(rdfWriter, graph, stream);
        }

        /// <summary>
        /// Writes the provided RDF graph to the specified stream using the given RDF writer.
        /// If no stream is provided, writes to standard output.
        /// </summary>
        /// <param name="writer">The RDF writer to use.</param>
        /// <param name="graph">The RDF graph to write.</param>
        /// <param name="stream">The output stream (optional).</param>
        public static void WriteGraph(IRdfWriter writer, IGraph graph, Stream stream = null)
        {
            stream = stream ?? Console.OpenStandardOutput();
            writer.Save(graph, new StreamWriter(stream), true);
        }
    }
}
