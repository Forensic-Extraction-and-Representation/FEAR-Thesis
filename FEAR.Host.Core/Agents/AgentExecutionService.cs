using FEAR.Domain.Agents;
using FEAR.Domain.Agents.Attachments;
using FEAR.Domain.Telemetry;
using FEAR.Host.Core.Services;
using FEAR.Runtime.Agents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;

namespace FEAR.Host.Core.Agents
{
    public class AgentExecutionService<T> : BackgroundService
    {
        private readonly IAgentAdapterQueueProvider _agentAdapterQueueProvider;
        private readonly IAgentAdapterRegistry _agentProvider;
        private readonly AgentAdapterContextProvider _contextProvider;
        private readonly IFEARTelemetrySignalService _telemetryService;
        private readonly InvestigationWorkspace _workspace;
        private readonly AgentExecutor _agentExecutor;
        private readonly QueryHelpRequestHandler _queryHelpHandler;
        private readonly AgentResponseFinalizer _responseFinalizer;

        public AgentExecutionService(
            IServiceProvider serviceProvider, 
            InvestigationWorkspace workspace,
            IAgentAdapterQueueProvider agentAdapterQueueProvider, 
            IAgentAdapterRegistry agentChainLoader,
            AgentAdapterContextProvider contextProvider, 
            IFEARTelemetrySignalService telemetryService)
        {
            _workspace = workspace;
            _agentProvider = agentChainLoader;
            _contextProvider = contextProvider;
            _telemetryService = telemetryService;
            _agentAdapterQueueProvider = agentAdapterQueueProvider;

            // Initialize provider classes
            _agentExecutor = new AgentExecutor(agentChainLoader, telemetryService);
            _queryHelpHandler = new QueryHelpRequestHandler(workspace);
            _responseFinalizer = new AgentResponseFinalizer(contextProvider, telemetryService);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_agentAdapterQueueProvider.TryDequeue(out var request))
                {
                    _telemetryService.SendSignal(new GenericFEARTelemetrySignal(AgentAdapterConstants.TelemetrySource, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Telemetry)
                        .WithSignalData($"Processing agent task for context ID: {request.Context.CtxId} for user: {request.Context.UserName}"));
                    await RunChatInference(request);
                }

                await Task.Delay(2000, cancellationToken); // Adjust the delay as needed
            }
        }

        internal async Task RunChatInference(QueuedAgentRequest request)
        {
            AgentInteraction agentResponse = CreateResponseEntry(request.Context, request.PromptContext);
            AgentExecutionContext executionContext = request.Context;
            AgentInteraction promptContext = executionContext.AgentInteractionInContext(request.PromptContext);
            executionContext.UpdateAgentInteraction(agentResponse);

            if(promptContext == null)
            {
                _telemetryService.SendSignal(new GenericFEARTelemetrySignal(AgentAdapterConstants.TelemetrySource, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Error)
                    .WithSignalData($"Prompt context entry not found in execution context for context ID: {request.Context.CtxId}"));

                FinalizeErrorResponse(request.Context, promptContext, agentResponse, new Exception("Prompt context entry not found."));
                return;
            }

            try
            {
                bool isRequestingQueryHelp = request.PromptContext.Flags?.ContainsKey("QueryHelp") == true && 
                                              (bool)request.PromptContext.Flags["QueryHelp"];

                if (isRequestingQueryHelp)
                {
                    await ProcessQueryHelpRequest(executionContext, promptContext, agentResponse);
                }
                else
                {
                    await ProcessAgentChain(executionContext, promptContext, agentResponse);
                }

                FinalizeSuccessResponse(executionContext, promptContext, agentResponse);
            }
            catch (Exception ex)
            {
                FinalizeErrorResponse(executionContext, promptContext, agentResponse, ex);
            }
        }

        private AgentInteraction CreateResponseEntry(AgentExecutionContext executionContext, AgentInteraction promptInteraction)
        {
            return new AgentInteraction
            {
                CtxId = executionContext.CtxId,
                CtxEntryId = Guid.NewGuid().ToString(),
                InResponseToCtEntryxId = promptInteraction.CtxEntryId,
                IsComplete = false,
                RequestType = ExecutionRequestType.Agent
            };
        }

        private async Task ProcessQueryHelpRequest(AgentExecutionContext executionContext, AgentInteraction promptInteraction, AgentInteraction agentResponse)
        {
            IChatAgentAdapter agentInterface = _workspace.AgentAdapterProvider.GetChatAgentAdapter("QueryHelper");
            agentResponse.Message = "Processing query help request...";
            agentResponse.AgentId = agentInterface.AgentId;
            var requestContextEntry = executionContext.AgentInteractionInContext(promptInteraction);

            await _queryHelpHandler.HandleQueryHelpRequest(executionContext, promptInteraction, agentResponse);
        }

        private async Task ProcessAgentChain(AgentExecutionContext executionContext, AgentInteraction promptInteraction, AgentInteraction agentResponse)
        {
            // Get all available agents
            var availableAgents = _agentProvider.GetAgents().ToList();

            if (availableAgents == null || !availableAgents.Any())
            {
                agentResponse.AddAgentMessage("System", "System", "No agents configured for this case.");
                return;
            }

            var promptContext = executionContext.AgentInteractionInContext(promptInteraction);

            // Initialize the response dictionary with prompt and graph
            var responseDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "<prompt>", promptContext.Message }
            };

            // Add graph if available
            if (promptContext.Attachments?.FirstOrDefault(a => a is GraphAttachment) is GraphAttachment graphAttachment)
            {
                responseDictionary["<graph>"] = graphAttachment.Result;
            }

            // Track agents that haven't been executed yet
            var remainingAgents = new List<AgentDefinition>(availableAgents);
            remainingAgents = remainingAgents.Where(t => t.AgentName != "QueryHelper").ToList(); // Exclude QueryHelper from execution if present, as it's a special case
            var executedAgentNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Track execution chain metadata
            var chainTracker = new AgentExecutionChainTracker();

            int totalAgents = availableAgents.Count;
            int executedCount = 0;

            // Feedforward execution: keep trying to execute agents until no more can be executed
            bool agentWasExecuted;
            do
            {
                agentWasExecuted = false;

                // Try to execute each remaining agent
                for (int i = remainingAgents.Count - 1; i >= 0; i--)
                {
                    var agentDef = remainingAgents[i];

                    // Check if all required inputs are available
                    if (CanExecuteAgent(agentDef, responseDictionary))
                    {
                        executedCount++;

                        // Execute the agent
                        var agentExecutionResponse = await _agentExecutor.ExecuteAgentWithDictionary(
                            executionContext, 
                            promptInteraction, 
                            agentDef, 
                            responseDictionary);

                        if (agentExecutionResponse != null)
                        {
                            // Add the agent's response to the dictionary using its name
                            responseDictionary[agentDef.AgentName] = agentExecutionResponse.Content;
                            executedAgentNames.Add(agentDef.AgentName);
                            agentWasExecuted = true;

                            // Record execution in chain tracker
                            var consumedInputs = agentDef.InputChain?.ToList() ?? new List<string>();
                            chainTracker.RecordExecution(
                                agentDef.AgentName,
                                agentExecutionResponse.AgentId,
                                consumedInputs,
                                executedCount,
                                agentExecutionResponse.CompletionTimestamp);

                            _telemetryService.SendSignal(new GenericFEARTelemetrySignal(
                                AgentAdapterConstants.TelemetrySource,
                                FEARTelemetrySerializationEnum.Raw,
                                FEARTelemetrySignalTypeEnum.Telemetry)
                                .WithSignalData($"Agent {agentDef.AgentName} executed successfully. Output added to response dictionary."));
                        }

                        // Remove from remaining agents
                        remainingAgents.RemoveAt(i);
                    }
                }
            } while (agentWasExecuted && remainingAgents.Any());

            // Log any agents that couldn't be executed
            if (remainingAgents.Any())
            {
                var unexecutedAgentNames = string.Join(", ", remainingAgents.Select(a => a.AgentName));
                _telemetryService.SendSignal(new GenericFEARTelemetrySignal(
                    AgentAdapterConstants.TelemetrySource,
                    FEARTelemetrySerializationEnum.Raw,
                    FEARTelemetrySignalTypeEnum.Warning)
                    .WithSignalData($"The following agents could not be executed due to missing inputs: {unexecutedAgentNames}"));
            }

            // Get terminal agents (outputs not consumed by other agents)
            var terminalAgents = chainTracker.GetTerminalAgents();

            if (terminalAgents.Any())
            {
                // Add only terminal agent messages to the response
                foreach (var terminalAgent in terminalAgents)
                {
                    var content = responseDictionary[terminalAgent.AgentName];
                    agentResponse.AddAgentMessage(
                        terminalAgent.AgentId,
                        terminalAgent.AgentName,
                        content,
                        new Dictionary<string, object>
                        {
                            { "CompletionTimestamp", terminalAgent.CompletionTimestamp },
                            { "ExecutionOrder", terminalAgent.ExecutionOrder },
                            { "IsTerminal", true },
                            { "OutputConsumed", false },
                            { "ConsumedInputs", string.Join(", ", terminalAgent.ConsumedInputs) }
                        });
                }

                _telemetryService.SendSignal(new GenericFEARTelemetrySignal(
                    AgentAdapterConstants.TelemetrySource,
                    FEARTelemetrySerializationEnum.Raw,
                    FEARTelemetrySignalTypeEnum.Telemetry)
                    .WithSignalData($"Terminal agents: {string.Join(", ", terminalAgents.Select(a => a.AgentName))}. Total executed: {executedCount}"));
            }
            else if (executedCount > 0)
            {
                // Fallback: if all agents were consumed, return the last agent's output
                var lastExecution = chainTracker.GetAllExecutions().LastOrDefault();
                if (lastExecution != null)
                {
                    agentResponse.AddAgentMessage(
                        lastExecution.AgentId,
                        lastExecution.AgentName,
                        responseDictionary[lastExecution.AgentName],
                        new Dictionary<string, object>
                        {
                            { "CompletionTimestamp", lastExecution.CompletionTimestamp },
                            { "ExecutionOrder", lastExecution.ExecutionOrder },
                            { "IsTerminal", false },
                            { "OutputConsumed", true },
                            { "Note", "All outputs were consumed; returning last execution" }
                        });
                }
            }
            else
            {
                // No agents executed
                agentResponse.AddAgentMessage("System", "System", "No agents were executed.");
            }
        }

        /// <summary>
        /// Determines if an agent can be executed based on available inputs in the response dictionary.
        /// </summary>
        private bool CanExecuteAgent(AgentDefinition agentDef, Dictionary<string, string> responseDictionary)
        {
            if (agentDef.InputChain == null || agentDef.InputChain.Length == 0)
            {
                return false;
            }

            // Check if all inputs are available
            foreach (var input in agentDef.InputChain)
            {
                if (!responseDictionary.ContainsKey(input))
                {
                    return false;
                }
            }

            return true;
        }

        private void FinalizeSuccessResponse(
            AgentExecutionContext executionContext, 
            AgentInteraction promptContext,
            AgentInteraction agentResponse)
        {
            _responseFinalizer.FinalizeSuccessResponse(executionContext, promptContext, agentResponse);
        }

        private void FinalizeErrorResponse(
            AgentExecutionContext executionContext, 
            AgentInteraction promptContext,
            AgentInteraction responseContext, 
            Exception ex)
        {
            _responseFinalizer.FinalizeErrorResponse(executionContext, promptContext, responseContext, ex);
        }
    }
}
