namespace FEAR.Domain.Agents.Attachments
{
    /// <summary>
    /// Abstract base class for content that includes a cryptographic signature.
    /// </summary>
    public abstract class SignedAgentAttachment : AgentAttachment
    {
        /// <summary>
        /// Gets or sets the cryptographic signature of the content.
        /// </summary>
        public string Signature { get; set; }
    }
}
