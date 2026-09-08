using System.Security.Cryptography;

namespace FEAR.Domain.Helpers
{
    /// <summary>
    /// Provides cryptographic helper methods for computing common hash digests (SHA-512, SHA-256, SHA-1, MD5)
    /// over byte arrays and streams. Uses reusable static instances of hash algorithms for efficiency.
    /// </summary>
    public static class CryptoHelpers
    {
        private static SHA512 sha = SHA512.Create();
        private static SHA256 sha256 = SHA256.Create();
        private static SHA1 sha1 = SHA1.Create();
        private static MD5 md5 = MD5.Create();

        /// <summary>
        /// Computes the SHA-512 hash of the given byte array.
        /// </summary>
        /// <param name="data">The input data to hash.</param>
        /// <returns>The SHA-512 hash as a byte array.</returns>
        public static byte[] Sha512(byte[] data) => ComputeHash(data, sha);

        /// <summary>
        /// Computes the SHA-256 hash of the given byte array.
        /// </summary>
        /// <param name="data">The input data to hash.</param>
        /// <returns>The SHA-256 hash as a byte array.</returns>
        public static byte[] Sha256(byte[] data) => ComputeHash(data, sha256);

        /// <summary>
        /// Computes the SHA-1 hash of the given byte array.
        /// </summary>
        /// <param name="data">The input data to hash.</param>
        /// <returns>The SHA-1 hash as a byte array.</returns>
        public static byte[] Sha1(byte[] data) => ComputeHash(data, sha1);

        /// <summary>
        /// Computes the MD5 hash of the given byte array.
        /// </summary>
        /// <param name="data">The input data to hash.</param>
        /// <returns>The MD5 hash as a byte array.</returns>
        public static byte[] Md5(byte[] data) => ComputeHash(data, md5);

        /// <summary>
        /// Computes the SHA-512 hash of the data from the given stream.
        /// </summary>
        /// <param name="s">The input stream to hash.</param>
        /// <returns>The SHA-512 hash as a byte array.</returns>
        public static byte[] Sha512(Stream s) => ComputeHash(s, sha);

        /// <summary>
        /// Computes the SHA-256 hash of the data from the given stream.
        /// </summary>
        /// <param name="s">The input stream to hash.</param>
        /// <returns>The SHA-256 hash as a byte array.</returns>
        public static byte[] Sha256(Stream s) => ComputeHash(s, sha256);

        /// <summary>
        /// Computes the SHA-1 hash of the data from the given stream.
        /// </summary>
        /// <param name="s">The input stream to hash.</param>
        /// <returns>The SHA-1 hash as a byte array.</returns>
        public static byte[] Sha1(Stream s) => ComputeHash(s, sha1);

        /// <summary>
        /// Computes the MD5 hash of the data from the given stream.
        /// </summary>
        /// <param name="s">The input stream to hash.</param>
        /// <returns>The MD5 hash as a byte array.</returns>
        public static byte[] Md5(Stream s) => ComputeHash(s, md5);

        /// <summary>
        /// Computes the hash of the data from the given stream using the specified hash algorithm.
        /// </summary>
        /// <param name="s">The input stream to hash.</param>
        /// <param name="hashAlgorithm">The hash algorithm to use.</param>
        /// <returns>The computed hash as a byte array.</returns>
        private static byte[] ComputeHash(Stream s, HashAlgorithm hashAlgorithm)
        {
            var hash = hashAlgorithm.ComputeHash(s);
            return hash;
        }

        /// <summary>
        /// Computes the hash of the given byte array using the specified hash algorithm.
        /// </summary>
        /// <param name="data">The input data to hash.</param>
        /// <param name="hashAlgorithm">The hash algorithm to use.</param>
        /// <returns>The computed hash as a byte array.</returns>
        private static byte[] ComputeHash(byte[] data, HashAlgorithm hashAlgorithm)
        {
            var hash = hashAlgorithm.ComputeHash(data);
            return hash;
        }
    }
}