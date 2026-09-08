using FEAR.Domain.Agents;
using FEAR.Domain.Agents.Attachments;
using FEAR.Domain.Telemetry;
using OpenAI.Chat;

namespace FEAR.Runtime.Agents
{
    [RegisteredAgentAdapterField("AgentModel", "Optional model name to override the default model for this agent.")]
    [RegisteredAgentAdapterField("ApiKey", "Required API key for authenticating with the OpenAI API.", isSecret: true)]
    [RegisteredAgentAdapterField("SystemPrompt", "Optional system prompt to override the default system prompt for this agent.")]
    [RegisteredAgentAdapterField("TemplatePrompt", "Optional template prompt to override the default template prompt for this agent. Use &lt;Prompt&gt; and &lt;Graph&gt; to include the user prompt and graph data, respectively.")]
    [RegisteredAgentAdapterField("AgentBaseUrl", "The base URL for the OpenWebAI instance, e.g. http://localhost:8000/", true)]
    public abstract class OpenAIApiAgent : AbstractChatAgent<OpenAIApiAgentModel>
    {
        protected readonly Lazy<ChatClient> _chatClient = null; 
        protected ChatClient ChatClient => _chatClient.Value;
        protected virtual string DefaultModel => "gpt-4o";

        public OpenAIApiAgent(AgentDefinition entry, Dictionary<string, object> options,
            IFEARTelemetrySignalService telemetryService) : base(entry, options, telemetryService)
        {
            _chatClient = new Lazy<ChatClient>(() => new ChatClient(AgentOptions.AgentModel, new System.ClientModel.ApiKeyCredential(AgentOptions.ApiKey), new OpenAI.OpenAIClientOptions()
            {
                Endpoint = new Uri(AgentOptions.AgentBaseUrl)
            }));
        }

        protected override OpenAIApiAgentModel ParseAgentModel(AgentDefinition entry, Dictionary<string, object> options)
        {
            var model = new OpenAIApiAgentModel();

            model.TemplatePrompt = GetValueOrDefault(options, "TemplatePrompt", string.Empty);
            model.SystemPrompt = GetValueOrDefault(options, "SystemPrompt", string.Empty);
            model.ApiKey = GetValueOrDefault(options, "ApiKey", string.Empty, true);
            model.AgentModel = GetValueOrDefault(options, "AgentModel", DefaultModel);
            model.AgentBaseUrl = GetValueOrDefault(options, "AgentBaseUrl", string.Empty, true);

            return model;
        }

        public override async Task<AgentResponse> AgentExecuteAsync(AgentExecutionContext ctx, AgentInteraction promptContext, SystemChatMessage systemMessage = null)
        {
            List<ChatMessage> cm = new List<ChatMessage>() { systemMessage ?? new SystemChatMessage(ChatMessageContentPart.CreateTextPart(AgentOptions.SystemPrompt)) };

            foreach (var entry in ctx.AgentInteractions)
            {
                if (entry.RequestType == ExecutionRequestType.User)
                {
                    // Add graph attachments
                    var graphAttachments = entry.Attachments.OfType<GraphAttachment>().Where(a => a.Result != null);
                    foreach (var graphAttachment in graphAttachments)
                    {
                        cm.Add(new UserChatMessage(ChatMessageContentPart.CreateTextPart("Begin Trusted Graph\n\n" + graphAttachment.Result + "End Trusted Graph\n\n")));
                    }
                    cm.Add(new UserChatMessage(ChatMessageContentPart.CreateTextPart(entry.Message)));
                }
                else if (entry.RequestType == ExecutionRequestType.Agent)
                {
                    if (!string.IsNullOrEmpty(entry.Message))
                        cm.Add(new AssistantChatMessage(entry.Message));
                }
            }

            try
            {
                System.ClientModel.ClientResult<ChatCompletion> completion = await ChatClient.CompleteChatAsync(cm.ToArray());

                AgentResponse cir = new AgentResponse()
                {
                    CompletionTimestamp = completion.Value.CreatedAt,
                    Content = String.Join(". ", completion.Value.Content.Where(t => t.Kind == ChatMessageContentPartKind.Text).Select(t => t.Text).ToList())
                };
                return cir;
            }
            catch (Exception ex)
            {
                _telemetryService.SendSignal(new GenericFEARTelemetrySignal(AgentAdapterConstants.TelemetrySource, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Exception)
                    .WithSignalData($"Exception during ChatInference: {ex.ToString()}"));
                return new AgentResponse()
                {
                    CompletionTimestamp = DateTime.Now,
                    Content = $"We encountered an error when attempting to run the Agent '{AgentName}'."
                };
            }
        }
    }
}
