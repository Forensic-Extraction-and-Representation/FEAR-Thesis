using FEAR.Domain.Model.ProtectedEncryptionKey;

namespace FEAR.Host.Core.ProtectedEncryptionKey
{
    /// <summary>
    /// Defines a contract for managing protected encryption keys and performing encryption/decryption operations.
    /// Provides methods for key setup, key submission, and cryptographic operations using named keys.
    /// </summary>
    public interface IProtectedEncryptionKeyProvider
    {
        /// <summary>
        /// Gets a value indicating whether key setup is enabled in the system.
        /// </summary>
        bool KeySetupEnabled { get; }
        string DefaultKeyName { get; }

        /// <summary>
        /// Adds an encrypted key to the provider with the specified name and identifiers.
        /// </summary>
        /// <param name="keyName">The name of the key.</param>
        /// <param name="keyIdentifiers">A dictionary of key identifiers (e.g., labels, IDs).</param>
        /// <param name="key">The encrypted key bytes.</param>
        void AddEncryptedKey(string keyName, IDictionary<string, string> keyIdentifiers, byte[] key);

        /// <summary>
        /// Attempts to decrypt a key with the specified name and identifiers.
        /// </summary>
        /// <param name="keyName">The name of the key to decrypt.</param>
        /// <param name="keyIdentifiers">A dictionary of key identifiers.</param>
        /// <returns>True if decryption is successful; otherwise, false.</returns>
        bool AttemptDecrypt(string keyName, IDictionary<string, string> keyIdentifiers);

        /// <summary>
        /// Submits a key to the provider for use in decryption or unlocking operations.
        /// </summary>
        /// <param name="data">The key submission model containing key details.</param>
        /// <returns>True if the key is accepted and usable; otherwise, false.</returns>
        bool SubmitKey(SubmitKeyModel data);

        /// <summary>
        /// Decrypts the specified data using the named key.
        /// </summary>
        /// <param name="data">The encrypted data to decrypt.</param>
        /// <param name="keyName">The name of the key to use for decryption.</param>
        /// <returns>The decrypted data as a byte array.</returns>
        byte[] DecryptData(byte[] data, string keyName);

        /// <summary>
        /// Encrypts the specified data using the named key.
        /// </summary>
        /// <param name="data">The plain data to encrypt.</param>
        /// <param name="keyName">The name of the key to use for encryption.</param>
        /// <returns>The encrypted data as a byte array.</returns>
        byte[] EncryptData(byte[] data, string keyName);
    }
}
