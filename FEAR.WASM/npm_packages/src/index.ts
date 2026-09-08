import * as sigma from "sigma";
import EdgeCurveProgram, { EdgeCurvedArrowProgram } from "@sigma/edge-curve";
import forceAtlas2 from "graphology-layout-forceatlas2";
import FA2Layout from "graphology-layout-forceatlas2/worker";
import FA2LayoutSupervisor from "graphology-layout-forceatlas2/worker";
import ForceSupervisor from "graphology-layout-force/worker";
import MultiGraph from "graphology";
import { fitViewportToNodes } from "@sigma/utils";
import Sigma from "sigma";

// Define the shape of the result object for better type safety
type GraphResult = {
    Graph: MultiGraph;
    Layout: any;
    Instance: Sigma;
    DotNetReference: any;
    AssignDotNetReference: (dotNetRef: any) => void;
    AddEdge: (s: string, o: string, f?: any) => void;
    Refresh: () => void;
    ResetCamera: () => void;
    GraphFunction: (f: string, ...args: any[]) => any;
    InstanceFunction: (f: string, ...args: any[]) => any;
    LayoutFunction: (f: string, ...args: any[]) => any;
    RunLayout: () => void;
    FuncCall: (obj: any, f: string, ...args: any[]) => any;
    ClearGraph: () => void;
};

declare global {
    interface Window {
        initializeGraph: (container: string) => Promise<any>;
        getContainerSize: (container: string) => { x: number; y: number };
    }
    const DotNet: any;
}

window.getContainerSize = function (container: string): { x: number; y: number } {
    const ct = document.getElementById(container) as HTMLElement;
    return { x: ct.parentElement.clientWidth, y: ct.parentElement.clientHeight };
}

window.initializeGraph = function (container: string): any {
    // Create a graphology.js graph, use random to init the graph
    const graph = new MultiGraph();
    let dotNetObject: any = undefined;

    // Graphology provides an easy to use implementation of Force Atlas 2 in a web worker
    const sensibleSettings = forceAtlas2.inferSettings(graph);

    const fa2Layout = new FA2Layout(graph, <any>{
        settings: {
            ...sensibleSettings,
            worker: true,
            autoStop: true,
            background: true,
            scaleRatio: 10,
            slowDown: 5,
            gravity: 1,
        }
    });

    const ct = document.getElementById(container) as HTMLElement;
    // Instantiate sigma.js and render the graph
    const renderer = new Sigma(graph, ct, <any>{
        allowInvalidContainer: true,
        defaultEdgeType: "curvedArrow",
        isNodeFixed: (_: any, attr: any) => attr.highlighted,
        renderEdgeLabels: true,
        edgeProgramClasses: {
            curvedArrow: EdgeCurvedArrowProgram,
        },
        //maxCameraRatio: 2,
        itemSizesReference: "screen",
        zoomToSizeRatioFunction: (x) => x,
        autoRescale: true
    });


    // State for drag'n'drop
    let draggedNode: string | null = null;
    let isDragging = false;

    // On mouse down on a node
    renderer.on("downNode", (e: any) => {
        isDragging = true;
        draggedNode = e.node;
        graph.setNodeAttribute(draggedNode, "highlighted", true);
        if (!renderer.getCustomBBox()) renderer.setCustomBBox(renderer.getBBox());
    });

    // On mouse move, if the drag mode is enabled, we change the position of the draggedNode
    renderer.on("moveBody", ({ event }: any) => {
        if (!isDragging || !draggedNode) return;

        // Get new position of node
        const pos = renderer.viewportToGraph(event);

        graph.setNodeAttribute(draggedNode, "x", pos.x);
        graph.setNodeAttribute(draggedNode, "y", pos.y);

        // Prevent sigma to move camera:
        event.preventSigmaDefault();
        event.original.preventDefault();
        event.original.stopPropagation();
    });

    // On mouse up, we reset the dragging mode
    const handleUp = () => {
        if (draggedNode) {
            graph.removeNodeAttribute(draggedNode, "highlighted");
        }
        isDragging = false;
        draggedNode = null;
    };

    let nodeClickMenuOpen = false;
    const handleStageClick = (e) => {
        if (nodeClickMenuOpen) {
            // Close the menu
        }
    }

    const handleNodeClick = (e) => {
        // Needs to come from blazor and assigned to the result object
        dotNetObject.invokeMethodAsync("AddText", e.node, e.event);
        if (nodeClickMenuOpen) {
            // Reposition and reset
        }
        console.log(e);
    }

    renderer.on("upNode", handleUp);
    renderer.on("upStage", handleUp);
    renderer.on("clickNode", handleNodeClick);
    renderer.on("clickStage", handleStageClick);
    
    const result: GraphResult = {
        Graph: graph,
        Layout: fa2Layout,
        Instance: renderer,
        DotNetReference: undefined,
        AssignDotNetReference: function (dotNetRef: any) {
            this.DotNetReference = dotNetRef;
            dotNetObject = dotNetRef;
        },
        AddEdge: function (s: string, o: string, f?: any) {
            this.Graph.addEdge(s, o, f);
        },
        ClearGraph: function () {
            this.Graph.clear();
        },
        Refresh: function () {
            this.Instance.refresh();
        },
        ResetCamera: function () {
            fitViewportToNodes(this.Instance, this.Graph.nodes(), { animate: true });
        },
        GraphFunction: function (f: string, ...args: any[]) {
            return this.FuncCall(this.Graph, f, ...args);
        },
        InstanceFunction: function (f: string, ...args: any[]) {
            return this.FuncCall(this.Instance, f, ...args);
        },
        LayoutFunction: function (f: string, ...args: any[]) {
            return this.FuncCall(this.Layout, f, ...args);
        },
        RunLayout: function () {
            if (this.Layout !== undefined && typeof this.Layout.start === "function") {
                this.Layout.start();
                setTimeout(() => {
                    this.Layout.stop();
                }, 1000);
            }
        },
        FuncCall: function (obj: any, f: string, ...args: any[]) {
            if (typeof obj[f] === "function") {
                return obj[f](...args);
            } else {
                throw new Error(`Function ${f} does not exist on ${obj}`);
            }
        },
    };
    const ret = DotNet.createJSObjectReference(result);
    return ret;
};