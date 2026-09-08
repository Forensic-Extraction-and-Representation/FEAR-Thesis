namespace FEAR.Domain.Agents
{
    public class AgentExecutionContext
    {
        public string CtxId { get; set; }

        public List<AgentInteraction> AgentInteractions { get; set; }
        public String Signature { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsComplete { get; set; }

        public AgentInteraction AgentInteractionInContext(AgentInteraction contextEntry)
        {
            if (AgentInteractions == null)
            {
                return null;
            }

            return AgentInteractions.FirstOrDefault(e => e.CtxEntryId == contextEntry.CtxEntryId);
        }

        public bool UpdateAgentInteraction(AgentInteraction entry)
        {
            if (AgentInteractions == null)
            {
                AgentInteractions = new List<AgentInteraction>();
            }

            // Remove existing entry with the same CtxEntryId
            if (AgentInteractions.Any(e => e.CtxEntryId == entry.CtxEntryId))
            {
                // Update the existing entry
                var existingEntry = AgentInteractions.First(e => e.CtxEntryId == entry.CtxEntryId);
                if (existingEntry.Signature != entry.Signature)
                {
                    existingEntry.Message = entry.Message;
                    existingEntry.RequestType = entry.RequestType;
                    existingEntry.Signature = entry.Signature;
                    existingEntry.AgentId = entry.AgentId;
                    existingEntry.StartTimestamp = entry.StartTimestamp;
                    existingEntry.EndTimestamp = entry.EndTimestamp;
                    return true;
                }
            }
            else
            {
                AgentInteractions.Add(entry);
                return true;
            }

                return false;
        }
    }
}
