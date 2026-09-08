namespace FEAR.Domain.Model.ProtectedEncryptionKey
{
    /// <summary>
    /// Represents an entry for automatic decryption using a named key.
    /// This class is used to associate a key name with a set of key-value pairs (such as parameters or secrets)
    /// that are required for decryption operations. It is typically used in scenarios where decryption
    /// must be performed automatically using pre-configured or discovered keys and their associated values.
    /// </summary>
    public class AutoDecryptionEntry
    {
        /// <summary>
        /// Gets or sets the name of the key used for decryption.
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of partial keys required for decryption.
        /// </summary>
        public Dictionary<string, string> Values { get; set; }
    }
}
