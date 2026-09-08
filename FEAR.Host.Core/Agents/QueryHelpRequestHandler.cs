using FEAR.Domain.Agents;
using FEAR.Host.Core.Services;
using FEAR.Runtime.Agents;

namespace FEAR.Host.Core.Agents
{
    /// <summary>
    /// Handles specialized processing for QueryHelper agent requests (SPARQL query generation).
    /// </summary>
    public class QueryHelpRequestHandler
    {
        private readonly InvestigationWorkspace _workspace;

        public QueryHelpRequestHandler(InvestigationWorkspace workspace)
        {
            _workspace = workspace;
        }

        /// <summary>
        /// Processes a query help request to generate SPARQL queries based on user prompts.
        /// </summary>
        public async Task HandleQueryHelpRequest(AgentExecutionContext executionContext, AgentInteraction promptContext, AgentInteraction agentResponse)
        {
            var agentInterface = _workspace.AgentAdapterProvider.GetChatAgentAdapter("QueryHelper");
            var graphHelpPrompt = promptContext.Message;

            string defaultInstruction = $"""
                You are an expert in generating semantic web SPARQL queries and helping people create their own.
                The user has asked for a sparql query that meets requirements described.
                Ensure the query is complete and includes all required prefixes.
                The ontology definition is included.
                Where there is a relationship, ensure you extract enough data to be able to draw visual links between the entities.
                You must only use terms that are within the ontology definition.
                The query must be a CONSTRUCT query, subjects must be entities and all relevant literals in the CONSTRUCT clause.
                Predicates to RDF definitions of the entities for their type must be included in the CONSTRUCT clause. 
                When related entities are included in the WHERE clause, they must also be present in the CONSTRUCT clause.
                Ensure that literals of related entities are also included in the CONSTRUCT clause.
                Return only the query.
                The users request is:
                "<prompt>"

                The ontology definition is below. You must only use terms defined in the ontology:
                <ontology>
                """;

            var instruction = agentInterface.InterpolateTemplatePrompt(new Dictionary<string, string>
            {
                { "prompt", graphHelpPrompt },
                { "ontology", _workspace.InvestigationOntology }
            },
            defaultInstruction: defaultInstruction);

            var internalPromptContext = new AgentInteraction
            {
                CtxId = executionContext.CtxId,
                CtxEntryId = Guid.NewGuid().ToString(),
                RequestType = ExecutionRequestType.Agent,
                IsComplete = false,
                Message = instruction,
                InResponseToCtEntryxId = null
            };

            AgentExecutionContext inferenceContext = new AgentExecutionContext
            {
                CtxId = Guid.NewGuid().ToString(),
                UserName = executionContext.UserName,
                AgentInteractions = new List<AgentInteraction>
                {
                    internalPromptContext
                },
                IsComplete = false
            };

            AgentResponse internalAgentResponse = await agentInterface.AgentExecuteAsync(inferenceContext, internalPromptContext);

            // Add the QueryHelper's response to the messages list
            agentResponse.AddAgentMessage(
                agentInterface.AgentId,
                "QueryHelper",
                internalAgentResponse.Content,
                new Dictionary<string, object>
                {
                    { "CompletionTimestamp", internalAgentResponse.CompletionTimestamp }
                });
        }
    }
}
