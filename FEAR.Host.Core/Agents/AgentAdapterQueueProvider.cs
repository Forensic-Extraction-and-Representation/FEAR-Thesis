using System.Collections.Concurrent;

namespace FEAR.Host.Core.Agents
{
    public class AgentAdapterQueueProvider : IAgentAdapterQueueProvider
    {
        private readonly ConcurrentQueue<QueuedAgentRequest> _queue = new();
        public void Enqueue(QueuedAgentRequest context) => _queue.Enqueue(context);
        public bool TryDequeue(out QueuedAgentRequest context)=> _queue.TryDequeue(out context);
    }
}
