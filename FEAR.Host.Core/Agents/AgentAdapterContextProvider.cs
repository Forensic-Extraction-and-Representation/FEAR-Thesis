using FEAR.Host.Core.Services;
using Microsoft.Extensions.Configuration;
using System.Text;
using FEAR.Domain.Agents;
using FEAR.Domain.Agents.Attachments;

namespace FEAR.Host.Core.Agents
{
    public class AgentAdapterContextProvider
    {
        private readonly IConfiguration _configuration;
        private readonly DataSigningService _dataSigningService;
        private IDictionary<string, AgentExecutionContext> _contexts = new Dictionary<string, AgentExecutionContext>();

        public AgentAdapterContextProvider(IConfiguration configuration, DataSigningService signingService)
        {
            _dataSigningService = signingService;
            _configuration = configuration;
        }

        public AgentExecutionContext CreateContext(string username, string caseName)
        {
            var ctxId = Guid.NewGuid().ToString();
            var context = new AgentExecutionContext
            {
                CtxId = ctxId,
                AgentInteractions = new List<AgentInteraction>(),
                UserName = username
            };

            _contexts[ctxId] = context;
            return context;
        }

        public AgentExecutionContext? GetContext(string ctxId, string username)
        {
            _contexts.TryGetValue(ctxId, out var context);
            if (context == null || !context.UserName.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                return null; // Context not found, or not for user
            }

            return context;
        }

        public bool TryGetContext(string ctxId, string username, out AgentExecutionContext? context)
        {
            context = GetContext(ctxId, username);

            return context != null;
        }

        public AgentExecutionContext AddEntryToContext(string ctxId, string username, AgentInteraction entry)
        {
            if (TryGetContext(ctxId, username, out var context))
            {
                entry.Signature = SignAgentInteraction(entry, _dataSigningService);
                entry.CtxId = ctxId;
                context.AgentInteractions.Add(entry);

                SignAgentContext(context, _dataSigningService);
                return context;
            }

            return null;
        }

        public AgentExecutionContext GenerateIntermediateContext(AgentExecutionContext parentContext)
        {
            var intermediateContext = new AgentExecutionContext
            {
                CtxId = Guid.NewGuid().ToString(),
                AgentInteractions = new List<AgentInteraction>(),
                UserName = parentContext.UserName
            };
            // Optionally, you can copy some entries from the parent context to the intermediate context
            // For example, you might want to copy the last entry or specific types of entries
            if (parentContext.AgentInteractions.Count > 0)
            {
                var lastEntry = parentContext.AgentInteractions.Last();
                intermediateContext.AgentInteractions.Add(lastEntry);
            }

            return intermediateContext;
        }

        public void AddInteractionToIntermediateContext(AgentExecutionContext intermediateContext, AgentInteraction entry)
        {
            entry.Signature = SignAgentInteraction(entry, _dataSigningService);
            intermediateContext.AgentInteractions.Add(entry);

        }

        public void UpdateAgentInteraction(string ctxId, string username, AgentInteraction entry, Func<AgentInteraction, String> entrySigningFunction)
        {
            if (TryGetContext(ctxId, username, out var context))
            {
                var existingEntry = context.AgentInteractions.FirstOrDefault(e => e.CtxEntryId == entry.CtxEntryId);
                if (existingEntry != null)
                {
                    existingEntry.Message = entry.Message;
                    existingEntry.Attachments = entry.Attachments;
                    existingEntry.RequestType = entry.RequestType;
                    existingEntry.Signature = entry.Signature;
                    existingEntry.IsComplete = entry.IsComplete;
                    existingEntry.EndTimestamp = entry.EndTimestamp;
                }
                else
                {
                    entry.Signature = entrySigningFunction(entry);
                    context.AgentInteractions.Add(entry);
                }
            }
        }

        protected Func<AgentInteraction, string> _chatContextEntrySignatureData =
            (entry) =>
            entry.Message + entry.CtxEntryId + entry.CtxId +
            string.Join("", entry.Attachments.OfType<SignedAgentAttachment>().Select(a => a.Signature ?? string.Empty)) +
            entry.RequestType.ToString() ?? string.Empty;

        public virtual AgentExecutionContext SignAgentContext(AgentExecutionContext ctx, DataSigningService signingService)
        {
            if (ctx == null || ctx.AgentInteractions.Count == 0)
            {
                throw new ArgumentException("Chat context cannot be null or empty.");
            }

            StringBuilder contextEntryHashes = new StringBuilder();

            foreach (var entry in ctx.AgentInteractions)
            {
                // Append the hash of the entry to the context entry hashes
                contextEntryHashes.Append(entry.Signature);
            }

            // Generate a signature for the entire context based on the concatenated hashes of the entries
            ctx.Signature = signingService.GenerateSignature(contextEntryHashes.ToString());
            return ctx;
        }

        public virtual string SignAgentInteraction(AgentInteraction entry, DataSigningService signingService)
        {
            if (entry == null)
            {
                throw new ArgumentException("Chat context entry cannot be null.");
            }

            string data = _chatContextEntrySignatureData(entry);

            return signingService.GenerateSignature(data);
        }
    }
}
