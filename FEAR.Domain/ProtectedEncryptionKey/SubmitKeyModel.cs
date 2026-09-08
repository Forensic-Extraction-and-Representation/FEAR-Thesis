namespace FEAR.Domain.Model.ProtectedEncryptionKey
{
    /// <summary>
    /// Represents the data model for submitting a single encryption key to the system.
    /// This model is used when the system requires a key to be entered online before it can unlock and continue loading.
    /// It supports grouping keys, naming, and associating identifiers and values for secure key management.
    /// </summary>
    public class SubmitKeyModel
    {
        /// <summary>
        /// Gets or sets the group identifier for the key, allowing keys to be logically grouped (e.g., by purpose or context).
        /// </summary>
        public string keyGroupIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the name of the key being submitted.
        /// It assumes that authorized users will all use the same key name.
        /// </summary>
        public string keyName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the key (e.g., key ID or label).
        /// </summary>
        public string keyIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the value of the key to be submitted for decryption or unlocking.
        /// </summary>
        public string keyValue { get; set; }
    }
}
