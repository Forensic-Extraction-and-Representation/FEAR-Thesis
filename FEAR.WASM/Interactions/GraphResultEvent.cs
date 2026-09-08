using FEAR.Domain.Agents.Querying;

namespace FEAR.WASM.Interactions
{
    public class GraphResultEvent
    {
        public GraphResultEvent(GraphResult graphResult, bool addAsLayer)
        {
            AddAsLayer = addAsLayer;
            GraphResult = graphResult;
        }

        public GraphResult GraphResult { get; }
        public bool AddAsLayer { get; set; }
    }
}
