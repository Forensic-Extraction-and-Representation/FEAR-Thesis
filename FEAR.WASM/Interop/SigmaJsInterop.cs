using FEAR.WASM.Components;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Implementation;
using FEAR.Blazor.Shared.Interop;
using System.Drawing;

namespace FEAR.WASM.Interop
{
    public class SigmaJsInterop : InteropBase
    {
        JSObjectReference _sigmaJsModule;
        private static readonly Random Random = new Random();
        private readonly string _containerName;
        private Point _containerDimensions;
        public SigmaJsInterop(IJSRuntime jsRuntime, string containerName) : base(jsRuntime)
        {
            _containerName = containerName;
        }

        public async Task InitializeAsync()
        {
            if (_sigmaJsModule != null)
                return;

            _containerDimensions = await JSRuntime.InvokeAsync<Point>("getContainerSize", _containerName);
            _sigmaJsModule = await JSRuntime.InvokeAsync<JSObjectReference>("initializeGraph", _containerName);
        }

        public async Task AddNode(string id, string label, int size, string color = "blue")
        {
            var node = new
            {
                label = label,
                x = Random.NextDouble(),
                y = Random.NextDouble(),
                size = size,
                color = color,
                curvature = 0.1
            };

            // This will need to be handled locally but we'll call the actual function for now

            if (!await HasNode(id))
                await _sigmaJsModule.InvokeVoidAsync("GraphFunction", "addNode", id, node);
        }

        public async Task<bool> HasNode(string id)
        {
            return await _sigmaJsModule.InvokeAsync<bool>("GraphFunction", "hasNode", id);
        }

        public async Task AddEdge(string source, string target, string label,int edgeCount, int size, string color)
        {
            var attributes = new
            {
                label = label,
                size = size,
                color = color,
                curved = true,
                curvature = edgeCount * 0.1
            };

            await _sigmaJsModule.InvokeVoidAsync("GraphFunction", "addEdge", source, target, attributes);
        }

        public async Task Layout()
        {
            await _sigmaJsModule.InvokeVoidAsync("RunLayout");
        }

        public async Task Refresh(bool skipIndexation)
        {
            await _sigmaJsModule.InvokeVoidAsync("InstanceFunction", "refresh", new { skipIndexation });
            await _sigmaJsModule.InvokeVoidAsync("ResetCamera");
        }

        internal async Task AssignDoNetReference(DotNetObjectReference<GraphContainer> dotNetReference)
        {
            await _sigmaJsModule.InvokeVoidAsync("AssignDotNetReference", dotNetReference);
        }

        internal async Task ClearGraph()
        {
            await _sigmaJsModule.InvokeVoidAsync("ClearGraph");
        }
    }
}
