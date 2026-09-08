using FEAR.Domain.Model.ProtectedEncryptionKey;

namespace FEAR.Host.Core.ProtectedEncryptionKey
{
    /// <summary>
    /// Represents configuration options for building and managing protected encryption keys.
    /// Used to control key setup, auto-decryption, and web administration features.
    /// </summary>
    public class ProtectedEncryptionKeyBuilderOptions
    {
        /// <summary>
        /// Gets or sets the path to the key configuration file.
        /// </summary>
        public string KeyConfigurationFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether key setup is enabled.
        /// </summary>
        public bool KeySetupEnabled { get; set; }

        /// <summary>
        /// Gets or sets the list of entries for automatic decryption using pre-configured keys.
        /// </summary>
        public List<AutoDecryptionEntry> AutoDecryptSet { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the web admin interface for key management is enabled.
        /// </summary>
        public bool EnableWebAdmin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the protected encryption key feature is enabled.
        /// </summary>
        public bool Enabled { get; set; }
    }
}
