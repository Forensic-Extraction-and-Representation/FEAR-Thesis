namespace FEAR.Domain.Agents
{
    /// <summary>
    /// Tracks metadata about agent execution and dependencies in a feedforward execution flow.
    /// </summary>
    public class AgentExecutionMetadata
    {
        /// <summary>
        /// The agent that was executed.
        /// </summary>
        public string AgentName { get; set; } = string.Empty;

        /// <summary>
        /// The agent's unique identifier.
        /// </summary>
        public string AgentId { get; set; } = string.Empty;

        /// <summary>
        /// Inputs that this agent consumed (names from the response dictionary).
        /// </summary>
        public List<string> ConsumedInputs { get; set; } = new List<string>();

        /// <summary>
        /// The output key that this agent produced (its AgentName).
        /// </summary>
        public string ProducedOutput { get; set; } = string.Empty;

        /// <summary>
        /// Whether this agent's output was consumed by another agent.
        /// </summary>
        public bool OutputWasConsumed { get; set; } = false;

        /// <summary>
        /// The order in which this agent was executed.
        /// </summary>
        public int ExecutionOrder { get; set; }

        /// <summary>
        /// When the agent completed execution.
        /// </summary>
        public DateTimeOffset CompletionTimestamp { get; set; }

        /// <summary>
        /// Whether this agent is a terminal node (output not consumed by others).
        /// </summary>
        public bool IsTerminalAgent => !OutputWasConsumed;
    }

    /// <summary>
    /// Tracks the execution chain and determines which agents produced final outputs.
    /// </summary>
    public class AgentExecutionChainTracker
    {
        private readonly Dictionary<string, AgentExecutionMetadata> _executionMetadata = new();

        /// <summary>
        /// Records that an agent was executed.
        /// </summary>
        public void RecordExecution(
            string agentName,
            string agentId,
            List<string> consumedInputs,
            int executionOrder,
            DateTimeOffset completionTimestamp)
        {
            var metadata = new AgentExecutionMetadata
            {
                AgentName = agentName,
                AgentId = agentId,
                ConsumedInputs = consumedInputs,
                ProducedOutput = agentName,
                ExecutionOrder = executionOrder,
                CompletionTimestamp = completionTimestamp,
                OutputWasConsumed = false
            };

            _executionMetadata[agentName] = metadata;

            // Mark any consumed agent outputs as consumed
            foreach (var input in consumedInputs)
            {
                // Skip system inputs
                if (input.StartsWith("<") && input.EndsWith(">"))
                    continue;

                if (_executionMetadata.TryGetValue(input, out var producerMetadata))
                {
                    producerMetadata.OutputWasConsumed = true;
                }
            }
        }

        /// <summary>
        /// Gets all agents whose outputs were not consumed by other agents (terminal agents).
        /// </summary>
        public List<AgentExecutionMetadata> GetTerminalAgents()
        {
            return _executionMetadata.Values
                .Where(m => m.IsTerminalAgent)
                .OrderBy(m => m.ExecutionOrder)
                .ToList();
        }

        /// <summary>
        /// Gets all execution metadata.
        /// </summary>
        public List<AgentExecutionMetadata> GetAllExecutions()
        {
            return _executionMetadata.Values
                .OrderBy(m => m.ExecutionOrder)
                .ToList();
        }

        /// <summary>
        /// Gets metadata for a specific agent.
        /// </summary>
        public AgentExecutionMetadata GetMetadata(string agentName)
        {
            return _executionMetadata.TryGetValue(agentName, out var metadata) ? metadata : null;
        }

        /// <summary>
        /// Checks if an agent's output was consumed.
        /// </summary>
        public bool WasOutputConsumed(string agentName)
        {
            return _executionMetadata.TryGetValue(agentName, out var metadata) && metadata.OutputWasConsumed;
        }
    }
}
