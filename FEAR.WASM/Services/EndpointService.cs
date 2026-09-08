using EventAggregator.Blazor;
using FEAR.Domain.Agents;
using FEAR.WASM.Domain;
using FEAR.WASM.Interactions;
using FEAR.WASM.Interop;

namespace FEAR.WASM.Services
{
    public class AgentExecutionRequest
    {
        public SessionMessage AgentResponseMessage { get; set; }
        public AgentInteraction AgentExecutionContextEntry { get; internal set; }
    }
    public class EndpointServiceState
    {
        public bool IncludeGraph { get; set; }
        public bool AddAsLayer { get; set; } = false;
        public AgentExecutionRequest ActiveAgentRequest { get; set; }
    }

    public class EndpointService : EventSubscriberService, IHandle<ToolbarEvent<bool>>
    {
        public EndpointServiceState State { get; } = new();
        private readonly AgentAdapterService _agentService;
        private readonly QueryService _queryService;
        private readonly InvestigationService _investigationService;
        private readonly MarkedInterop _markedInterop;
        private bool _isProcessing = false;

        public EndpointService(IEventAggregator eventAggregator, AgentAdapterService agentService,
            InvestigationService investigationService, MarkedInterop markedInterop, QueryService queryService)
            : base(eventAggregator)
        {
            _agentService = agentService;
            _queryService = queryService;
            _markedInterop = markedInterop;
            _investigationService = investigationService;

            System.Timers.Timer t = new System.Timers.Timer();
            t.Elapsed += async (s, e) =>
            {
                if (!_isProcessing)
                {
                    _isProcessing = true;
                    try
                    {
                        if (State.ActiveAgentRequest != null)
                        {
                            AgentInteraction dequeuedEntry = State.ActiveAgentRequest.AgentExecutionContextEntry;
                            var response = await _agentService.PollChatRequest(dequeuedEntry);

                            var agentMessage = State.ActiveAgentRequest.AgentResponseMessage;
                            var agentContent = response.Context.AgentInteractions.FirstOrDefault(t => t.InResponseToCtEntryxId == dequeuedEntry.CtxEntryId)?.Message;

                            agentMessage.Content = agentContent;
                            if (response.IsComplete)
                            {
                                agentMessage.State = SessionMessageState.COMPLETED;

                                agentMessage.AgentMessages.AddRange(response.Context.AgentInteractions.FirstOrDefault(t => t.InResponseToCtEntryxId == dequeuedEntry.CtxEntryId).Messages);

                                State.ActiveAgentRequest = null;

                                Mediator.PublishAsync(new GenericUIEvent(GenericUIEventConstants.SESSION_UPDATED));

                                Mediator.PublishAsync(new ResolvableStatusEvent()
                                {
                                    IsResolved = false,
                                    ResolveIdentifier = dequeuedEntry.CtxId,
                                    StatusText = "Executing agent request..."
                                });
                            }
                            else
                            {
                                Mediator.PublishAsync(new GenericUIEvent(GenericUIEventConstants.SESSION_UPDATED));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error polling agent request: {ex.Message}");
                    }
                    finally
                    {
                        _isProcessing = false;
                    }
                }
            };

            t.Interval = 2000;
            t.Start();
        }

        public async Task HandleAsync(ToolbarEvent<bool> notification)
        {
            if (notification.Option == "IncludeGraph")
                State.IncludeGraph = notification.Value;
        }

        internal async Task SendAgentMessage(SendSessionEvent message, SessionMessage agentMessage)
        {
            var contextEntry = await _agentService.ExecuteAgentRequest(message.SubmitData);

            Mediator.PublishAsync(new ResolvableStatusEvent()
            {
                IsResolved = false,
                ResolveIdentifier = contextEntry.CtxId,
                StatusText = "Executing agent request..."
            });

            State.ActiveAgentRequest = new AgentExecutionRequest
            {
                AgentExecutionContextEntry = contextEntry,
                AgentResponseMessage = agentMessage
            };
        }

        internal async Task SendQueryMessage(SendSessionEvent message, SessionMessage userMessage)
        {
            var resolvableId = Guid.NewGuid().ToString();

            Mediator.PublishAsync(new ResolvableStatusEvent()
            {
                IsResolved = false,
                ResolveIdentifier = resolvableId,
                StatusText = "Executing query request..."
            });

            var beginResponse = await _queryService.BeginQuery(message.SubmitData);
            var result = await _queryService.GetQueryResult(beginResponse);
            var graphResult = _queryService.ProcessQueryResponse(result);

            if (graphResult != null)
            {
                // Handle the graph result, e.g., update UI or state
                // This should be done in the Blazor component using JS interop for rendering
                Mediator.PublishAsync(new GraphResultEvent(graphResult, State.AddAsLayer));
            }

            Mediator.PublishAsync(new ResolvableStatusEvent()
            {
                IsResolved = true,
                ResolveIdentifier = resolvableId,
                StatusText = "Query executed successfully."
            });
        }
    }
}
