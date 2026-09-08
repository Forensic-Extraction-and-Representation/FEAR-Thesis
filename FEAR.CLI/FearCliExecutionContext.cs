using FEAR.CLI.Arguments;
using FEAR.Domain.KnowledgeGraph.GraphManager;
using FEAR.Domain.Telemetry;
using FEAR.Runtime.DataFormats;
using FEAR.Runtime.Execution;
using FEAR.Runtime.Helpers;
using KnowledgeGraph.Visualization;
using Microsoft.Extensions.Logging;
using Microsoft.Msagl.GraphViewerGdi;
using System.Net;

namespace FEAR.CLI
{
    public class FearCliExecutionContext : FearExecutionContext<FearCliArguments>
    {
        private DataFormatContextFactory _dataFormatContextFactory = new DataFormatContextFactory();

        public FearCliExecutionContext(IFEARTelemetrySignalService telemetryService, IFEARTelemetryDestinationProvider destinationProvider, IServiceProvider serviceProvider, IGraphManager graphManager, FearExecutionOptions<FearCliArguments> executionArguments, Runtime.Environment fearEnvironment)
            : base(telemetryService, destinationProvider, serviceProvider, graphManager, executionArguments, fearEnvironment)
        {
        }

        private void ProcessData(String fileName, IDataFormatContext dataFormatContext)
        {
            // If we have a web service address set, create a new web request for the data.
            if (!string.IsNullOrEmpty(ExecutionOptions.TypedArguments.WebServiceAddressOption?.Value))
            {
                string url = ExecutionOptions.TypedArguments.WebServiceAddressOption.Value;
                string contentType = dataFormatContext.ContentMimeType;
                
                using FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = contentType;
                request.ContentLength = file.Length;

                using Stream dataStream = request.GetRequestStream();
                file.CopyTo(dataStream);
                dataStream.Close();

                using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using Stream responseStream = response.GetResponseStream();
                using StreamReader reader = new StreamReader(responseStream);
                string responseFromServer = reader.ReadToEnd();
            }
            else
            {
                // else, run local
                IDataFormatContextState state = dataFormatContext.CreateStateFromFile(fileName);

                var execCtx = new NonReturningArtifactPipelineExecution(state);
                Execute(execCtx);
            }
        }

        public void Execute()
        {
            if (ExecutionOptions.TypedArguments.TestCompileOption.Value)
                return;

            IDataFormatContext dataFormatContext = _dataFormatContextFactory.Create(ExecutionOptions.TypedArguments.InputFormatOption.Value);

            if (dataFormatContext is NoDataFormatContext)
            {
                FEARHostedTelemetry.SendExecutionTelemetry($"No data format context found for {ExecutionOptions.TypedArguments.InputFormatOption.Value}", TelemetryService, FEARTelemetrySignalTypeEnum.Exception);
                return;
            }

            // Open the artifacts directory and get all the artifacts, the FormatOption will tell us how to read the data
            List<string> fileList = new List<string>();
            if (File.Exists(ExecutionOptions.TypedArguments.GetArtifactsDirectory()))
            {
                fileList.Add(ExecutionOptions.TypedArguments.GetArtifactsDirectory());
            }
            else
            {
                foreach (string fileExtension in dataFormatContext.FileExtensionFilter)
                {
                    fileList.AddRange(Directory.GetFiles(ExecutionOptions.TypedArguments.GetArtifactsDirectory(), fileExtension, SearchOption.AllDirectories));
                }
            }

            // Foreach each artifact file, read the data and pass it to the gfear script
            foreach (var file in fileList)
            {
                ProcessData(file, dataFormatContext);
            }

            List<Stream> outputStream = new List<Stream>();
            if (ExecutionOptions.TypedArguments.ConsoleOutputOption.Value)
                outputStream.Add(Console.OpenStandardOutput());

            if (ExecutionOptions.TypedArguments.OutputFileOption.Value != null)
            {
                outputStream.Add(new FileStream(ExecutionOptions.TypedArguments.OutputFileOption.Value, FileMode.Create));
            }

            GraphHelpers.WriteGraph(ExecutionOptions.TypedArguments.OutputSerializationOption.Value, GraphManager.MaterializedGraph.Graph, outputStream);

            if (ExecutionOptions.TypedArguments.VisualizeOption.Value)
            {
                //GraphVizWriter gvw = new GraphVizWriter();
                //gvw.Save(GraphManager.MaterializedGraph.Graph, @"C:\temp\graph.dot");

                IGraphVisualizer gv = new PureRdfVisualizer();
                GViewer gviewer = gv.Visualize(GraphManager, new GraphVisualizerContext() { Colorise = false, BucketTimes = false, CompressTypes = false });
                Form f = new Form();
                f.Width = (int)(Screen.PrimaryScreen.Bounds.Width * 0.9);
                f.Height = (int)(Screen.PrimaryScreen.Bounds.Height * 0.75);
                f.Top = (Screen.PrimaryScreen.Bounds.Height - f.Height) / 3;
                f.Left = (Screen.PrimaryScreen.Bounds.Width - f.Width) / 2;
                gviewer.Dock = DockStyle.Fill;
                f.Controls.Add(gviewer);
                f.ShowDialog();
            }
        }
    }
}
