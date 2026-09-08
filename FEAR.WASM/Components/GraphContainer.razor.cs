using EventAggregator.Blazor;
using FEAR.Blazor.Shared.Interop;
using FEAR.WASM.Components.Dialogs;
using FEAR.WASM.Components.Graph;
using FEAR.WASM.Domain;
using FEAR.WASM.Interactions;
using FEAR.WASM.Interop;
using FEAR.WASM.Model;
using FEAR.WASM.Services;
using FEAR.WASM.Providers;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Extensions.Options;
using VDS.RDF;
using VDS.RDF.Query;
using static FEAR.WASM.Components.DataTable;
using FEAR.Domain.Model.ColorMap;

namespace FEAR.WASM.Components
{
    public class GraphLayer
    {
        public string Name { get; set; }
    }

    public class LayerTripleContext : VDS.RDF.BasicTripleContext
    {
        public LayerTripleContext(VDS.RDF.Graph graph)
        {
            this.Graph = graph;
        }
        public VDS.RDF.Graph Graph { get; }
        public List<GraphLayer> Layers { get; set; } = new List<GraphLayer>();

        public void AddLayer(GraphLayer layer)
        {
            if (!Layers.Contains(layer))
            {
                Layers.Add(layer);
            }
        }
    }

    public class NodeClickEventArgs
    {
        public float x { get; set; }
        public float y { get; set; }
    }

    public partial class GraphContainer : IHandle<GraphResultEvent>
    {
        private DataTableContainer dataTableContainerRef;
        private VDS.RDF.Graph containerGraph = new VDS.RDF.Graph();

        [Inject(Key = "MainGraphContainer")]
        protected SigmaJsInterop SigmaInterop { get; set; }

        [Inject]
        protected QueryService QueryService { get; set; }

        [Inject]
        protected IColorMapProvider ColorMapProvider { get; set; }

        IEnumerable<LegendItem> Legend => ColorMapProvider.GetLegentItems();
        Dictionary<string, GraphLayer> graphLayers = new Dictionary<string, GraphLayer>();

        protected override void OnInitialized()
        {
            // Initialize provider without IDs (caller can pass later if needed)
            // Keep default legend for now (will be replaced by provider data once initialized)
            // No-op: provider handles caches
        }

        public void SetColorMap(List<ColorMap> colorMaps)
        {
        }

        private ColorMap ResolveColor(string predicateUri, string subjectUri, string? objectValue)
            => ColorMapProvider.Resolve(predicateUri, subjectUri, objectValue);

        public async Task RenderGraph(VDS.RDF.Graph graph, bool addAsLayer)
        {
            if (!addAsLayer)
            {
                graphLayers.Clear();
                containerGraph.Clear();
                await SigmaInterop.ClearGraph();
            }

            var gl = new GraphLayer()
            {
                Name = "Layer_" + Guid.NewGuid().ToString("N").Substring(0, 8)
            };

            // Avoid blocking the WebAssembly UI thread — use a non-blocking delay
            await Task.Delay(500);

            graphLayers.Add(gl.Name, gl);
            await ProcessGraph(graph, gl);

            var dotNetReference = DotNetObjectReference.Create(this);
            await SigmaInterop.AssignDoNetReference(dotNetReference);

            // Ensure the sigma JS module has been initialized before invoking functions that rely on it
            try
            {
                await SigmaInterop.InitializeAsync();
            }
            catch (Exception initEx)
            {
                Console.Error.WriteLine($"SigmaInterop initialization failed: {initEx}");
            }

            // Call refresh/layout and surface any JS interop exceptions to the browser/console
            try
            {
                await SigmaInterop.Refresh(false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"SigmaInterop.Refresh failed: {ex}");
            }

            try
            {
                await SigmaInterop.Layout();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"SigmaInterop.Layout failed: {ex}");
            }
        }

        private async Task ProcessGraph(VDS.RDF.Graph graph, GraphLayer layer)
        {
            var parallelEdgeCount = new Dictionary<string, Dictionary<string, int>>();

            foreach (var triple in graph.Triples)
            {
                if (containerGraph.ContainsTriple(triple))
                {
                    var t = containerGraph.GetTriplesWithSubjectObject(triple.Subject, triple.Object).WithPredicate(triple.Predicate).First();
                    (t.Context as LayerTripleContext)?.AddLayer(layer);
                }
                else
                {
                    triple.Context = new LayerTripleContext(containerGraph);
                    (triple.Context as LayerTripleContext)?.AddLayer(layer);
                    containerGraph.Assert(triple);

                    var subjectNode = (triple.Subject as IUriNode);
                    var predicateNode = (triple.Predicate as IUriNode);
                    var objectNode = (triple.Object as IUriNode);

                    bool objIsLiteral = triple.Object is VDS.RDF.LiteralNode;
                    string objId = "";
                    if (objIsLiteral)
                        objId = (triple.Object as VDS.RDF.LiteralNode).Value;
                    else
                        objId = objectNode.Uri.ToString();

                    string subjectId = subjectNode.Uri.ToString();

                    string subjectLabel = ColorMapProvider.ReduceUri(subjectNode.Uri.ToString());
                    string predicate = ColorMapProvider.ReduceUri(predicateNode.Uri.ToString());

                    ColorMap predicateColor = ResolveColor(predicateNode.Uri.ToString(), "", "");
                    ColorMap subjectColor = ResolveColor(subjectNode.Uri.ToString(), "", "");
                    ColorMap objectColor = null;

                    if (objIsLiteral)
                    {
                        objectColor = ResolveColor(predicateNode.Uri.ToString(), subjectNode.Uri.ToString(), (triple.Object as VDS.RDF.LiteralNode).Value);
                    }
                    else if (triple.Object is IUriNode objNode)
                    {
                        objectColor = ResolveColor(predicateNode.Uri.ToString(), subjectNode.Uri.ToString(), objNode.Uri.ToString());
                    }

                    predicateColor ??= LegendStyleCache.DefaultColorMap;

                    await SigmaInterop.AddNode(subjectId, subjectLabel, 3, subjectColor.Styles.Node.BackgroundColor);

                    if (triple.Object is LiteralNode literal)
                    {
                        await SigmaInterop.AddNode(literal.Value, literal.Value, 3, objectColor.Styles.Node.BackgroundColor);
                        await SigmaInterop.AddEdge(subjectId, literal.Value, predicate, 1, 1, predicateColor.Styles.Edge.LineColor);
                    }
                    else
                    {
                        // Check if the pec[sub][obj] already has an edge count. We need to ensure that [obj][sub] is also added and reflects the same count. This is to ensure that parallel edges are drawn correctly.
                        IUriNode objNode = triple.Object as IUriNode;
                        int edgeCount = 1;
                        if (objNode != null)
                        {
                            if (!parallelEdgeCount.ContainsKey(subjectId))
                            {
                                parallelEdgeCount[subjectId] = new Dictionary<string, int>();
                            }
                            if (!parallelEdgeCount[subjectId].ContainsKey(objId))
                            {
                                parallelEdgeCount[subjectId][objId] = 0;
                            }
                            parallelEdgeCount[subjectId][objId]++;

                            if (!parallelEdgeCount.ContainsKey(objId))
                            {
                                parallelEdgeCount[objId] = new Dictionary<string, int>();
                            }
                            if (!parallelEdgeCount[objId].ContainsKey(subjectId))
                            {
                                parallelEdgeCount[objId][subjectId] = 0;
                            }

                            edgeCount = parallelEdgeCount[objId][subjectId] = parallelEdgeCount[subjectId][objId];
                        }

                        string objectLabel = ColorMapProvider.ReduceUri(objNode.Uri.ToString());
                        await SigmaInterop.AddNode(objId, objectLabel, 3, objectColor.Styles.Node.BackgroundColor);
                        await SigmaInterop.AddEdge(subjectId, objId, predicate, edgeCount, 1,predicateColor.Styles.Edge.LineColor);
                    }
                }
            }
        }

        [JSInvokable("AddText")]
        public void NodeClicked(string node, NodeClickEventArgs args)
        {
            Console.WriteLine($"Node clicked: {node}");
        }

        public async Task RenderTable(SparqlResultSet resultSet, bool addAsLayer)
        {
            dataTableContainerRef.AddDataTableConfig(new DataTableConfig
            {
                Columns = ["Column 1", "Column 2", "Column 3"],
                Data = new List<object[]>
                {
                    new object[] { "Value 1", "Value 2","Value 3" },
                    new object[] { "Value A", "Value B", "Value C" },
                    new object[] { "Value X", "Value Y", "Value Z" },
                    new object[] { "Value 4", "Value 5", "Value 6" },
                },
            });
            Console.WriteLine("Rendering table with result set");
            foreach (var row in resultSet.Results)
            {
                Console.WriteLine(string.Join(", ", row.Select(cell => cell.ToString())));
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await SigmaInterop.InitializeAsync();
        }

        public async Task HandleAsync(GraphResultEvent message)
        {
            if (message.GraphResult.Graphable)
                await RenderGraph(message.GraphResult.Graph, message.AddAsLayer);
            else
            {
                await RenderTable(message.GraphResult.ResultSet, message.AddAsLayer);
            }
        }
    }
}