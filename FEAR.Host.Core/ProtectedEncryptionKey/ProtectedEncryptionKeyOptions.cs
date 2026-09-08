using System.Text.Json;

namespace FEAR.Host.Core.ProtectedEncryptionKey
{
    /// <summary>
    /// Represents configuration options for protected encryption keys, including loading and saving from a file.
    /// </summary>
    class ProtectedEncryptionKeyOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedEncryptionKeyOptions"/> class.
        /// </summary>
        public ProtectedEncryptionKeyOptions() { }

        /// <summary>
        /// Gets or sets the list of protected encryption keys.
        /// </summary>
        public List<ProtectedEncryptionKey> Keys { get; set; }

        /// <summary>
        /// Loads <see cref="ProtectedEncryptionKeyOptions"/> from a JSON configuration file.
        /// </summary>
        /// <param name="keyConfigurationFile">The path to the configuration file.</param>
        /// <returns>The deserialized <see cref="ProtectedEncryptionKeyOptions"/> instance.</returns>
        public static ProtectedEncryptionKeyOptions Load(string keyConfigurationFile)
        {
            using (Stream stream = File.OpenRead(keyConfigurationFile))
                return JsonSerializer.Deserialize<ProtectedEncryptionKeyOptions>(stream);
        }

        /// <summary>
        /// Saves the <see cref="ProtectedEncryptionKeyOptions"/> to a JSON configuration file.
        /// </summary>
        /// <param name="keyConfigurationFile">The path to the configuration file.</param>
        /// <param name="options">The options instance to serialize and save.</param>
        public static void Save(string keyConfigurationFile, ProtectedEncryptionKeyOptions options)
        {
            using (Stream stream = File.OpenWrite(keyConfigurationFile))
                JsonSerializer.Serialize(stream, options);
        }
    }
}
