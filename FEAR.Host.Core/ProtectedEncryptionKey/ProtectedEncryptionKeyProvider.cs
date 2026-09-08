using FEAR.Domain.Model.ProtectedEncryptionKey;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;

namespace FEAR.Host.Core.ProtectedEncryptionKey
{
    /// <summary>
    /// Provides management and cryptographic operations for protected encryption keys.
    /// Handles key storage, encryption, decryption, and key submission using AES and key caching.
    /// </summary>
    public class ProtectedEncryptionKeyProvider : IProtectedEncryptionKeyProvider
    {
        private ProtectedEncryptionKeyBuilderOptions _encryptionKeyOptions;
        private Lazy<ProtectedEncryptionKeyOptions> _protectedEncryptionKeyOptions;
        private Dictionary<string, string> _keyCache = new Dictionary<string, string>();
        private MemoryCache _keySubmissionCache = new MemoryCache("ProtectedEncryptionKeyProvider");

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedEncryptionKeyProvider"/> class with the specified options.
        /// Loads or creates the key configuration file and attempts auto-decryption if configured.
        /// </summary>
        /// <param name="options">The builder options for encryption key management.</param>
        public ProtectedEncryptionKeyProvider(ProtectedEncryptionKeyBuilderOptions options)
        {
            _encryptionKeyOptions = options;

            if (options.Enabled)
            {
                _protectedEncryptionKeyOptions = new Lazy<ProtectedEncryptionKeyOptions>(() =>
                {
                    if (!File.Exists(_encryptionKeyOptions.KeyConfigurationFile))
                    {
                        ProtectedEncryptionKeyOptions.Save(_encryptionKeyOptions.KeyConfigurationFile, new ProtectedEncryptionKeyOptions() { Keys = new List<ProtectedEncryptionKey>() });
                    }

                    return ProtectedEncryptionKeyOptions.Load(_encryptionKeyOptions.KeyConfigurationFile);
                });

                if (_encryptionKeyOptions.AutoDecryptSet != null)
                {
                    foreach (var x in _encryptionKeyOptions.AutoDecryptSet)
                    {
                        AttemptDecrypt(x.KeyName, x.Values);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public bool KeySetupEnabled => _encryptionKeyOptions.KeySetupEnabled;

        /// <summary>
        /// Calculates the AES key and IV for encrypting or decrypting a key, based on key name and identifiers.
        /// </summary>
        private Tuple<byte[], byte[]> CalculateKeyEncryptionIV(string keyName, IDictionary<string, string> keyIdentifiers)
        {
            var keyOptions = _protectedEncryptionKeyOptions.Value.Keys.FirstOrDefault(k => k.KeyName == keyName);

            IDictionary<string, string> orderedKeys = new Dictionary<string, string>();
            if (keyOptions == null)
            {
                orderedKeys = keyIdentifiers;
            }
            else
            {
                foreach (var x in keyOptions.KeyIdentifiers)
                {
                    orderedKeys.Add(x, keyIdentifiers[x]);
                }
            }

            string allKeyValues = string.Join("", orderedKeys.Values);
            string keyValue = allKeyValues;
            while (keyValue.Length < 32)
            {
                keyValue = keyValue + keyValue;
            }

            return new Tuple<byte[], byte[]>(Encoding.UTF8.GetBytes(keyValue.Substring(0, 32)), MD5.HashData(Encoding.UTF8.GetBytes(allKeyValues)).Take(16).ToArray());
        }

        public string DefaultKeyName => _keyCache.First().Key;

        /// <summary>
        /// Constructs a new AES instance with CBC mode and PKCS7 padding.
        /// </summary>
        private Aes ConstructAes(string keyName)
        {
            Aes aes = Aes.Create();

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = aes.LegalKeySizes.Last().MaxSize;
            aes.BlockSize = aes.LegalBlockSizes.Last().MaxSize;

            return aes;
        }

        /// <inheritdoc/>
        public void AddEncryptedKey(string keyName, IDictionary<string, string> keyIdentifiers, byte[] key)
        {
            try
            {
                Aes aes = ConstructAes(keyName);
                var keyIV = CalculateKeyEncryptionIV(keyName, keyIdentifiers);
                ProtectedEncryptionKeyOptions options = _protectedEncryptionKeyOptions.Value;

                ICryptoTransform encryptor = aes.CreateEncryptor(keyIV.Item1, keyIV.Item2);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(key, 0, key.Length);
                        cs.Flush();
                        cs.FlushFinalBlock();

                        options.Keys.Add(new ProtectedEncryptionKey() { KeyName = keyName, KeyIdentifiers = keyIdentifiers.Keys.ToList(), EncryptedKey = Convert.ToBase64String(ms.ToArray()) });
                        ProtectedEncryptionKeyOptions.Save(_encryptionKeyOptions.KeyConfigurationFile, options);
                    }
                }
            }
            catch (Exception)
            {
                // Swallow exceptions to avoid breaking key addition flow
            }
        }

        /// <inheritdoc/>
        public bool AttemptDecrypt(string keyName, IDictionary<string, string> keyIdentifiers)
        {
            try
            {
                ProtectedEncryptionKeyOptions options = _protectedEncryptionKeyOptions.Value;

                // Check if the keyName exists in the options
                if (!options.Keys.Any(k => k.KeyName == keyName) || _keyCache.ContainsKey(keyName))
                {
                    throw new Exception("Invalid Request");
                }

                Aes aes = ConstructAes(keyName);
                var keyIV = CalculateKeyEncryptionIV(keyName, keyIdentifiers);

                ICryptoTransform decryptor = aes.CreateDecryptor(keyIV.Item1, keyIV.Item2);
                ProtectedEncryptionKey keyData = options.Keys.FirstOrDefault(k => k.KeyName == keyName);
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(keyData.EncryptedKey)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            _keyCache[keyName] = sr.ReadToEnd();
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public bool SubmitKey(SubmitKeyModel data)
        {
            lock (_keySubmissionCache)
            {
                try
                {
                    Dictionary<string, string> keyDict = new Dictionary<string, string>();
                    if (_keySubmissionCache.Contains(data.keyGroupIdentifier))
                    {
                        keyDict = _keySubmissionCache.Get(data.keyGroupIdentifier) as Dictionary<string, string>;
                    }

                    keyDict.Add(data.keyIdentifier, data.keyValue);

                    _keySubmissionCache.Set(data.keyGroupIdentifier, keyDict, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromMinutes(5) });
                    var decrypted = AttemptDecrypt(data.keyName, keyDict);
                    if (decrypted)
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    // Swallow exceptions to avoid breaking key submission flow
                }

                return false;
            }
        }

        /// <summary>
        /// Calculates the IV for encryption/decryption based on the key's encrypted value and name.
        /// </summary>
        private byte[] CalculateEncryptionIV(string keyName)
        {
            ProtectedEncryptionKeyOptions options = _protectedEncryptionKeyOptions.Value;

            var key = options.Keys.FirstOrDefault(options => options.KeyName == keyName);
            return MD5.HashData(Encoding.UTF8.GetBytes(key.EncryptedKey + key.KeyName)).Take(16).ToArray();
        }

        /// <inheritdoc/>
        public byte[] DecryptData(byte[] data, string keyName)
        {
            try
            {
                if (!_keyCache.ContainsKey(keyName))
                {
                    throw new Exception("Invalid Request");
                }

                Aes aes = ConstructAes(keyName);
                var key = Encoding.UTF8.GetBytes(_keyCache[keyName]);
                var iv = CalculateEncryptionIV(keyName);

                ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);
                using (MemoryStream ms = new MemoryStream(data))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream ms2 = new MemoryStream())
                        {
                            cs.CopyTo(ms2);
                            return ms2.ToArray();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public byte[] EncryptData(byte[] data, string keyName)
        {
            try
            {
                if (!_keyCache.ContainsKey(keyName))
                {
                    throw new Exception("Invalid Request");
                }

                Aes aes = ConstructAes(keyName);
                var key = Encoding.UTF8.GetBytes(_keyCache[keyName]);
                var iv = CalculateEncryptionIV(keyName);

                ICryptoTransform encryptor = aes.CreateEncryptor(key, iv);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Flush();
                        cs.FlushFinalBlock();
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
