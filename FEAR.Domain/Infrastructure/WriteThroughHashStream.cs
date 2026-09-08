using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FEAR.Domain.Infrastructure
{
    public class WriteThroughHashStream<T>
        : Stream where T : HashAlgorithm
    {
        private readonly Stream stream;
        private readonly HashAlgorithm hashAlgorithm;

        public WriteThroughHashStream(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            var hashType = typeof(T);
            // Get the static method "Create()" from HashAlgorithm type
            var createMethod = hashType.GetMethod("Create", new Type[] { });
            if (createMethod == null)
                throw new InvalidOperationException($"HashAlgorithm type {hashType.Name} does not have a static Create method.");

            // Invoke the Create method to get an instance of the HashAlgorithm
            hashAlgorithm = (HashAlgorithm)createMethod.Invoke(null, null);
        }

        public string Hash
        {
            get
            {
                // Finalize the hash computation and return the hash value
                hashAlgorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                return Convert.ToBase64String(hashAlgorithm.Hash);
            }
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => stream.CanWrite;
        public override long Length => stream.Length;
        public override long Position
        {
            get => stream.Position;
            set => throw new NotSupportedException("Position cannot be set on this stream.");
        }

        public override void Flush()
        {
            stream.Flush();
            hashAlgorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        }

        public void WriteSecret(byte[] secret)
        {
            // Add this to the hash only
            if (secret == null) throw new ArgumentNullException(nameof(secret));
            if (secret.Length == 0) throw new ArgumentException("Secret cannot be empty.", nameof(secret));

            hashAlgorithm.TransformBlock(secret, 0, secret.Length, null, 0);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Read operation is not supported on this stream.");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seek operation is not supported on this stream.");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("SetLength operation is not supported on this stream.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("Invalid offset or count.");
            hashAlgorithm.TransformBlock(buffer, offset, count, null, 0);
            stream.Write(buffer, offset, count);
        }
    }
}
