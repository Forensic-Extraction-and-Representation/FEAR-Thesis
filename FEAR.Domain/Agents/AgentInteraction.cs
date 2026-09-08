using FEAR.Domain.Agents.Attachments;

namespace FEAR.Domain.Agents
{
    /// <summary>
    /// Represents an entry in the execution context, which tracks the execution of a request or action within an agent system. 
    /// Each entry contains information about the context, timestamps, request type, flags, graph content, messages, and other relevant details. 
    /// This class is used to maintain a history of actions and their outcomes for auditing, debugging, or analysis purposes.
    /// </summary>
    public class AgentInteraction
    {
        public string CtxId { get; set; }
        public string CtxEntryId { get; set; }
        public DateTime StartTimestamp { get; set; }
        public DateTime? EndTimestamp { get; set; }
        public ExecutionRequestType RequestType { get; set; } = ExecutionRequestType.User;

        public Dictionary<string, bool> Flags { get; set; }

        public List<AgentAttachment> Attachments { get; set; } = new List<AgentAttachment>();

        /// <summary>
        /// List of messages from agents that contributed to this interaction.
        /// In feedforward execution, multiple agents may contribute to a single response.
        /// </summary>
        public List<AgentMessage> Messages { get; set; } = new List<AgentMessage>();

        /// <summary>
        /// Legacy single message property for backward compatibility.
        /// Returns the combined content of all messages or the last message's content.
        /// </summary>
        [Obsolete("Use Messages property instead. This property is maintained for backward compatibility.")]
        public string Message
        {
            get => Messages.LastOrDefault()?.Content ?? string.Empty;
            set
            {
                if (Messages.Count == 0)
                {
                    Messages.Add(new AgentMessage
                    {
                        AgentId = GetPrimaryAgentId(),
                        Content = value,
                        Timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    Messages[Messages.Count - 1].Content = value;
                }
            }
        }

        /// <summary>
        /// Legacy AgentId property for backward compatibility.
        /// Returns the ID of the last agent that contributed to this interaction.
        /// </summary>
        [Obsolete("AgentId is now tracked per message. Use Messages[].AgentId instead.")]
        public string AgentId
        {
            get => GetPrimaryAgentId();
            set
            {
                // For backward compatibility, update the last message's agent ID
                if (Messages.Count > 0)
                {
                    Messages[Messages.Count - 1].AgentId = value;
                }
            }
        }

        public bool IsComplete { get; set; } = true;
        public string Signature { get; set; } = string.Empty;
        public string InResponseToCtEntryxId { get; set; }

        /// <summary>
        /// Adds a message from an agent to this interaction.
        /// </summary>
        public void AddAgentMessage(string agentId, string agentName, string content, Dictionary<string, object> metadata = null)
        {
            Messages.Add(new AgentMessage
            {
                AgentId = agentId,
                AgentName = agentName,
                Content = content,
                Timestamp = DateTime.UtcNow,
                Metadata = metadata ?? new Dictionary<string, object>()
            });
        }

        /// <summary>
        /// Gets the primary agent ID (the last agent that contributed).
        /// </summary>
        public string GetPrimaryAgentId()
        {
            return Messages.LastOrDefault()?.AgentId ?? string.Empty;
        }

        /// <summary>
        /// Gets the combined message content from all agents.
        /// </summary>
        public string GetCombinedMessage(string separator = "\n\n")
        {
            return string.Join(separator, Messages.Select(m => $"[{m.AgentName}]: {m.Content}"));
        }

        /// <summary>
        /// Gets messages from terminal agents only (agents whose outputs weren't consumed by others).
        /// </summary>
        public List<AgentMessage> GetTerminalAgentMessages()
        {
            // If metadata is stored in the message, filter by IsTerminal flag
            return Messages.Where(m => 
                !m.Metadata.ContainsKey("OutputConsumed") || 
                !(bool)m.Metadata["OutputConsumed"]).ToList();
        }
    }
}
