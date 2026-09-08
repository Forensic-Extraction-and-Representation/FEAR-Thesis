using EventAggregator.Blazor;
using FEAR.WASM.Domain;
using FEAR.WASM.Interactions;
using FEAR.WASM.Interop;

namespace FEAR.WASM.Services
{
    public class SessionHistoryService : EventSubscriberService, IHandle<SendSessionEvent>, IHandle<ReceiveSessionEvent>
    {
        private readonly MarkedInterop _markedInterop;

        public List<SessionMessage> Messages { get; } = new List<SessionMessage>();

        private readonly EndpointService _endpointService;

        public SessionHistoryService(IEventAggregator eventAggregator, EndpointService endpointService, MarkedInterop markedInterop)
            : base(eventAggregator)
        {
            _endpointService = endpointService;
            _markedInterop = markedInterop;
        }

        public async Task HandleAsync(SendSessionEvent message)
        {
            SessionMessage omniMsg = new SessionMessage();

            try
            {
                if (message.RequestType == "query")
                {
                    omniMsg.Type = SessionMessageType.UserQuery;
                    omniMsg.Content = $"User executed query:\n```\n{message.SubmitData}\n```";
                    _endpointService.SendQueryMessage(message, omniMsg);
                    Messages.Add(omniMsg);
                }
                else if (message.RequestType == "chat")
                {
                    omniMsg.Type = SessionMessageType.UserChat;
                    omniMsg.Content = message.SubmitData;
                    var botMsg = new SessionMessage
                    {
                        Type = SessionMessageType.AgentResponse,
                        Content = "Generating..."
                    };

                    Messages.Add(omniMsg);
                    Messages.Add(botMsg);
                    await _endpointService.SendAgentMessage(message, botMsg);
                }

                Mediator.PublishAsync(new GenericUIEvent(GenericUIEventConstants.SESSION_UPDATED));
            }
            catch (Exception ex)
            {
                // Send "Executing Completed Event"
                omniMsg.State = SessionMessageState.FAILED;
            }
            finally
            {
                Mediator.PublishAsync(new GenericUIEvent(GenericUIEventConstants.SESSION_UPDATED));
            }
        }

        public async Task HandleAsync(ReceiveSessionEvent message)
        {
            if (message.Message == null)
                return;

            this.Messages.FirstOrDefault(m => m.Id == message.Message.Id)?.Update(message.Message);

            Mediator.PublishAsync(new GenericUIEvent(GenericUIEventConstants.SESSION_UPDATED));
        }
    }
}
