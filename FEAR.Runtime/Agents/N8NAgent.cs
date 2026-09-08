using FEAR.Domain.Agents;
using FEAR.Domain.Agents.Attachments;
using FEAR.Domain.Telemetry;
using OpenAI.Chat;
using OpenAI.Responses;
using System.Net.Http.Json;

namespace FEAR.Runtime.Agents
{
    [RegisteredAgentAdapter("N8N", AgentAdapterType.Action)]
    [RegisteredAgentAdapterField("AgentBaseUrl", "The base URL for the n8n instance, e.g. https://n8n.example.com/webhook/aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    [RegisteredAgentAdapterField("IncludeAttachments", "Whether to include attachments in the agent response. Defaults to false if not specified.")]
    [RegisteredAgentAdapterFieldOption("IncludeAttachments", "True", "true", "Include attachments in the agent response.")]
    [RegisteredAgentAdapterFieldOption("IncludeAttachments", "False", "false", "Do not include attachments in the agent response.")]

    public class N8NAgent : AbstractActionAgent<N8NAgentOptionModel>
    {
        protected override N8NAgentOptionModel ParseAgentModel(AgentDefinition entry, Dictionary<string, object> options)
        {
            var model = new N8NAgentOptionModel();

            model.AgentBaseUrl = GetValueOrDefault(options, "AgentBaseUrl", string.Empty, true);
            model.IncludeAttachments = bool.Parse(GetValueOrDefault(options, "IncludeAttachments", "false").ToLower());
            
            return model;
        }

        public N8NAgent(AgentDefinition entry, Dictionary<string, object> options, IFEARTelemetrySignalService telemetrySignalService) : base(entry, options, telemetrySignalService)
        {
        }


        public override async Task<AgentResponse> AgentExecuteAsync(AgentExecutionContext ctx, AgentInteraction promptContext)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(AgentOptions.AgentBaseUrl);

            JsonContent content = null;
            HttpResponseMessage response = null;
            AgentResponse agentResponse = null;

            content = JsonContent.Create(new { Prompt = promptContext.Message });
            HttpContent requestContent = content;
            if (AgentOptions.IncludeAttachments)
            {
                var multipartContent = new MultipartFormDataContent
                {
                    { content, "prompt" }
                };

                foreach (var attachment in promptContext.Attachments)
                {
                    multipartContent.Add(new StringContent(attachment.Result), "attachment", $"{attachment.Name}.{attachment.Format}");
                }

                requestContent = multipartContent;
            }
            
            response = await httpClient.PostAsync("", requestContent);

            if (response != null)
            {
                response.EnsureSuccessStatusCode();
                agentResponse = new AgentResponse
                {
                    AgentId = this.AgentId,
                    CompletionTimestamp = DateTimeOffset.UtcNow,
                    Content = await response.Content.ReadAsStringAsync()
                };
            }
            else
            {
                agentResponse = new AgentResponse
                {
                    AgentId = this.AgentId,
                    CompletionTimestamp = DateTimeOffset.UtcNow,
                    Content = string.Empty
                };
            }

            return agentResponse;
        }
    }
}
