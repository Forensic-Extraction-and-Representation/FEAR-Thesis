namespace FEAR.Host.Core.ProtectedEncryptionKey
{
    /// <summary>
    /// Represents a protected encryption key, including its name, identifiers, and encrypted value.
    /// Used for managing and referencing encryption keys in secure storage or key management systems.
    /// </summary>
    class ProtectedEncryptionKey
    {
        /// <summary>
        /// Gets or sets the name of the encryption key.
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// Gets or sets the list of identifiers associated with the key (e.g., labels, tags, or key IDs).
        /// </summary>
        public List<string> KeyIdentifiers { get; set; }

        /// <summary>
        /// Gets or sets the encrypted key value (typically Base64-encoded).
        /// </summary>
        public string EncryptedKey { get; set; }
    }
}
