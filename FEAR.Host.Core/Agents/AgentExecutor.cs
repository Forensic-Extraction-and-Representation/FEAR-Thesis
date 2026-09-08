using FEAR.Domain.Agents;
using FEAR.Domain.Agents.Attachments;
using FEAR.Domain.Telemetry;
using FEAR.Runtime.Agents;

namespace FEAR.Host.Core.Agents
{
    /// <summary>
    /// Responsible for executing individual agents and managing their lifecycle.
    /// </summary>
    public class AgentExecutor
    {
        private readonly IAgentAdapterRegistry _agentProvider;
        private readonly IFEARTelemetrySignalService _telemetryService;

        public AgentExecutor(
            IAgentAdapterRegistry agentProvider, 
            IFEARTelemetrySignalService telemetryService)
        {
            _agentProvider = agentProvider;
            _telemetryService = telemetryService;
        }

        /// <summary>
        /// Executes a single agent using inputs from the response dictionary.
        /// </summary>
        public async Task<AgentResponse> ExecuteAgentWithDictionary(
            AgentExecutionContext executionContext,
            AgentInteraction promptInteraction,
            AgentDefinition agentDefinition,
            Dictionary<string, string> responseDictionary)
        {
            IAgentAdapter agentInterface = _agentProvider.GetAgentAdapter(
                agentDefinition.AgentIdentifier, 
                RequestedAgentAdapterType.Any);

            var agentChainOptions = agentInterface.GetAgentOptions();

            if (!ValidateAgentInputChain(agentInterface, agentChainOptions, executionContext.CtxId))
            {
                return null;
            }

            // Build the agent execution context from the response dictionary
            var agentExecutionContext = BuildExecutionContextFromDictionary(
                executionContext,
                promptInteraction,
                agentDefinition,
                responseDictionary);

            // Get the last context entry to pass to the agent
            var contextEntryForExecution = agentExecutionContext.AgentInteractions.LastOrDefault();
            if (contextEntryForExecution == null)
            {
                _telemetryService.SendSignal(new GenericFEARTelemetrySignal(
                    AgentAdapterConstants.TelemetrySource,
                    FEARTelemetrySerializationEnum.Raw,
                    FEARTelemetrySignalTypeEnum.Warning)
                    .WithSignalData($"Agent {agentInterface.AgentId} has no valid context entry. Skipping execution for context ID: {executionContext.CtxId}"));
                return null;
            }

            AgentResponse agentResponse = await agentInterface.AgentExecuteAsync(agentExecutionContext, contextEntryForExecution);
            agentResponse.AgentId = agentInterface.AgentId;

            return agentResponse;
        }

        /// <summary>
        /// Builds an execution context for the agent using inputs from the response dictionary.
        /// </summary>
        private AgentExecutionContext BuildExecutionContextFromDictionary(
            AgentExecutionContext originalContext,
            AgentInteraction promptInteraction,
            AgentDefinition agentDefinition,
            Dictionary<string, string> responseDictionary)
        {
            var agentExecutionContext = new AgentExecutionContext
            {
                CtxId = Guid.NewGuid().ToString(),
                UserName = originalContext.UserName,
                AgentInteractions = new List<AgentInteraction>(),
                IsComplete = false
            };

            // Build context entries from the input chain
            foreach (var inputName in agentDefinition.InputChain)
            {
                if (responseDictionary.TryGetValue(inputName, out var inputValue))
                {
                    var contextEntry = new AgentInteraction
                    {
                        CtxId = agentExecutionContext.CtxId,
                        CtxEntryId = Guid.NewGuid().ToString(),
                        Message = inputValue,
                        AgentId = inputName.Equals("prompt", StringComparison.OrdinalIgnoreCase) 
                            ? promptInteraction.AgentId 
                            : inputName,
                        RequestType = ExecutionRequestType.Agent,
                        IsComplete = true,
                        Flags = promptInteraction.Flags
                    };

                    // If this is a graph input, try to add it as an attachment
                    if (inputName.Equals("graph", StringComparison.OrdinalIgnoreCase) && 
                        promptInteraction.Attachments?.FirstOrDefault(a => a is GraphAttachment) is GraphAttachment graphAttachment)
                    {
                        contextEntry.Attachments = new List<AgentAttachment> { graphAttachment };
                    }

                    agentExecutionContext.AgentInteractions.Add(contextEntry);
                }
            }

            return agentExecutionContext;
        }

        /// <summary>
        /// Validates that the agent has a valid input chain configuration.
        /// </summary>
        private bool ValidateAgentInputChain(IAgentAdapter agentInterface, AgentDefinition agentOptions, string contextId)
        {
            if (agentOptions.InputChain == null || agentOptions.InputChain.Length == 0)
            {
                _telemetryService.SendSignal(new GenericFEARTelemetrySignal(
                    AgentAdapterConstants.TelemetrySource, 
                    FEARTelemetrySerializationEnum.Raw, 
                    FEARTelemetrySignalTypeEnum.Warning)
                    .WithSignalData($"Agent {agentInterface.AgentId} has no input chain defined. Skipping inference for context ID: {contextId}"));
                return false;
            }

            return true;
        }
    }
}

