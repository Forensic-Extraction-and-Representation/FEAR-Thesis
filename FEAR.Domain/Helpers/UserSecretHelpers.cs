using FEAR.Domain.Dto.Authentication;
using System.Text;
using System.Text.Json;

namespace FEAR.Domain.Helpers
{
    /// <summary>
    /// Provides helper methods for encrypting and decrypting user secret information.
    /// Supports serialization and deserialization of user secrets using custom encryption and decryption callbacks.
    /// </summary>
    public class UserSecretHelpers
    {
        /// <summary>
        /// Delegate for encrypting data using a specified key name.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="keyName">The name of the encryption key.</param>
        /// <returns>The encrypted data as a byte array.</returns>
        public delegate byte[] EncryptionCallback(byte[] data, string keyName);

        /// <summary>
        /// Delegate for decrypting data using a specified key name.
        /// </summary>
        /// <param name="encryptedData">The encrypted data to decrypt.</param>
        /// <param name="keyName">The name of the decryption key.</param>
        /// <returns>The decrypted data as a byte array.</returns>
        public delegate byte[] DecryptionCallback(byte[] encryptedData, string keyName);

        /// <summary>
        /// Decrypts and deserializes a <see cref="UserSecret"/> into a <see cref="FEAR.Domain.Model.Authentication.UserSecretInfo"/> object.
        /// </summary>
        /// <param name="secret">The user secret containing the encrypted data and key name.</param>
        /// <param name="decryptionCallback">The callback used to decrypt the secret data.</param>
        /// <returns>The deserialized <see cref="FEAR.Domain.Model.Authentication.UserSecretInfo"/> object.</returns>
        public static FEAR.Domain.Model.Authentication.UserSecretInfo UnlockRecord(UserSecret secret, DecryptionCallback decryptionCallback)
        {
            return JsonSerializer.Deserialize<FEAR.Domain.Model.Authentication.UserSecretInfo>(decryptionCallback(Convert.FromBase64String(secret.SecretData), secret.KeyName));
        }

        /// <summary>
        /// Serializes and encrypts a <see cref="FEAR.Domain.Model.Authentication.UserSecretInfo"/> object for storage in a <see cref="UserSecret"/>.
        /// </summary>
        /// <param name="secretInfo">The user secret info to serialize and encrypt.</param>
        /// <param name="secret">The user secret containing the key name for encryption.</param>
        /// <param name="encryptionCallback">The callback used to encrypt the serialized data.</param>
        /// <returns>The encrypted and base64-encoded string suitable for storage.</returns>
        public static string LockRecord(FEAR.Domain.Model.Authentication.UserSecretInfo secretInfo, UserSecret secret, EncryptionCallback encryptionCallback)
        {
            string jsonData = JsonSerializer.Serialize(secretInfo);
            byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);
            byte[] encrytpedData = encryptionCallback(dataBytes, secret.KeyName);
            return Convert.ToBase64String(encrytpedData);
        }
    }
}
