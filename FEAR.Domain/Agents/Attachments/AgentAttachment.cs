using System.Text.Json.Serialization;

namespace FEAR.Domain.Agents.Attachments
{
    /// <summary>
    /// Base class for all attachment types in agent communication.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(AgentAttachment), typeDiscriminator: "attachment")]
    [JsonDerivedType(typeof(GraphAttachment), typeDiscriminator: "graphAttachment")]
    public class AgentAttachment
    {
        /// <summary>
        /// Gets or sets the result content of the attachment.
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Gets or sets the format of the result (e.g., JSON, XML, TTL).
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the name associated with the object.
        /// </summary>
        public string Name { get; set; }
    }
}
