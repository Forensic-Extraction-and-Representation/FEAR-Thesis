using FEAR.Domain.Agents.Attachments;

namespace FEAR.Domain.Agents.Querying
{
    public class GraphQueryAction : GraphQueryRequest
    {

        private Func<GraphAttachment>? queryAction = null;
        public Func<GraphAttachment>? QueryAction { get => queryAction; set => queryAction = value; }
    }
}
