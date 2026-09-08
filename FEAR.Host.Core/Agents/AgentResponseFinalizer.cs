using FEAR.Domain.Agents;
using FEAR.Domain.Telemetry;
using FEAR.Runtime.Agents;

namespace FEAR.Host.Core.Agents
{
    /// <summary>
    /// Responsible for finalizing agent responses and updating the context provider.
    /// </summary>
    public class AgentResponseFinalizer
    {
        private readonly AgentAdapterContextProvider _contextProvider;
        private readonly IFEARTelemetrySignalService _telemetryService;

        public AgentResponseFinalizer(
            AgentAdapterContextProvider contextProvider,
            IFEARTelemetrySignalService telemetryService)
        {
            _contextProvider = contextProvider;
            _telemetryService = telemetryService;
        }

        /// <summary>
        /// Finalizes a successful agent response by updating the response entry and context.
        /// </summary>
        public void FinalizeSuccessResponse(
            AgentExecutionContext executionContext,
            AgentInteraction promptContext,
            AgentInteraction agentResponse)
        {
            agentResponse.IsComplete = true;
            agentResponse.EndTimestamp = DateTime.UtcNow;

            _contextProvider.UpdateAgentInteraction(
                executionContext.CtxId,
                executionContext.UserName,
                agentResponse,
                (entry) => entry.Message + entry.CtxEntryId);

            executionContext.IsComplete = true;
        }

        /// <summary>
        /// Finalizes an error response by logging the error and updating the context.
        /// </summary>
        public void FinalizeErrorResponse(
            AgentExecutionContext executionContext,
            AgentInteraction promptContext,
            AgentInteraction responseEntry,
            Exception ex)
        {
            _telemetryService.SendSignal(new GenericFEARTelemetrySignal(
                AgentAdapterConstants.TelemetrySource,
                FEARTelemetrySerializationEnum.Raw,
                FEARTelemetrySignalTypeEnum.Error)
                .WithSignalData($"Error processing agent execution for context ID: {executionContext.CtxId}. Error: {ex.Message}"));

            // Add error message to the messages list
            responseEntry.AddAgentMessage("System", "System", $"An error occurred while processing the agent request: {ex.Message}");
            responseEntry.IsComplete = true;
            responseEntry.EndTimestamp = DateTime.UtcNow;

            _contextProvider.UpdateAgentInteraction(
                executionContext.CtxId,
                executionContext.UserName,
                responseEntry,
                (entry) => entry.Message + entry.CtxEntryId);

            executionContext.IsComplete = true;
        }
    }
}
