using EventAggregator.Blazor;
using FEAR.Domain.Agents;
using FEAR.WASM.Interactions;
using FEAR.WASM.Model;
using System.Net.Http.Json;

namespace FEAR.WASM.Services
{
    public class AgentAdapterService : EventSubscriberService, IHandle<ToolbarEvent<bool>>
    {
        private readonly WASMInternalHttpFactory _httpClientFactory;
        private readonly QueryService _queryService;
        private readonly WebUIConfiguration _configuration;

        public AgentAdapterServiceState State { get; } = new();

        public AgentAdapterService(WASMInternalHttpFactory httpClientFactory, QueryService queryService,
            IEventAggregator eventAggregator, WebUIConfiguration configuration)
            : base(eventAggregator)
        {
            _httpClientFactory = httpClientFactory;
            _queryService = queryService;
            _configuration = configuration;

            Mediator.Subscribe(this);
        }

        public async Task<AgentExecutionContext> ComposeContext()
        {
            if (State.ChatContext == null)
            {
                var chatUrl = $"v1.0/AgentAdapter/BeginExecution/{_configuration.InvestigationName}";
                var client = await _httpClientFactory.CreateHttpClientAsync();
                var response = await client.GetFromJsonAsync<AgentExecutionContext>(chatUrl);

                State.ChatContext = response;
            }

            return State.ChatContext;
        }

        public async Task<AgentInteraction> ExecuteAgentRequest(string chatInput)
        {
            var ctx = await ComposeContext();
            var chatUrl = $"v1.0/AgentAdapter/RunExecution/{_configuration.InvestigationName}";

            try
            {
                var client = await _httpClientFactory.CreateHttpClientAsync();
                var ctxEntry = new AgentInteraction()
                {
                    CtxEntryId = Guid.NewGuid().ToString(),
                    CtxId = ctx.CtxId,
                    Message = chatInput,
                    Flags = State.Options,
                    RequestType = ExecutionRequestType.User
                };

                // Add graph attachment if requested
                if (State.Options.ContainsKey("IncludeGraph") && (bool)State.Options["IncludeGraph"])
                {
                    var graphAttachment = _queryService.GetCurrentQueryResult();
                    if (graphAttachment != null)
                    {
                        ctxEntry.Attachments.Add(graphAttachment);
                    }
                }

                var response = await client.PostAsJsonAsync(chatUrl, ctxEntry);

                // If context not found (possible server restart), reset and retry
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    State.ChatContext = null;
                    ctx = await ComposeContext();
                    ctxEntry.CtxId = ctx.CtxId;

                    response = await client.PostAsJsonAsync(chatUrl, ctxEntry);
                }

                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<AgentInteraction>();
                if (data != null)
                {
                    State.ChatContext.UpdateAgentInteraction(data);
                }

                return data;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error sending agent request: {ex.Message}");
                return null;
            }
        }

        public class PollChatResponse
        {
            public AgentExecutionContext Context { get; set; }
            public bool IsComplete { get; set; }
        }

        public async Task<PollChatResponse> PollChatRequest(AgentInteraction ctxEntry)
        {
            var chatUrl = $"v1.0/AgentAdapter/PollExecution/{_configuration.InvestigationName}/{ctxEntry.CtxId}";

            try
            {
                var client = await _httpClientFactory.CreateHttpClientAsync();
                var response = await client.GetAsync(chatUrl);
                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<AgentExecutionContext>();
                var pollResponse = new PollChatResponse
                {
                    Context = data,
                    IsComplete = data?.IsComplete ?? false,
                };

                return pollResponse;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error sending agent request: {ex.Message}");
                return null;
            }
        }

        public async Task HandleAsync(ToolbarEvent<bool> notification)
        {
            if (!State.Options.ContainsKey(notification.Option))
                State.Options.Add(notification.Option, notification.Value);
            else
                State.Options[notification.Option] = notification.Value;
        }
    }
}