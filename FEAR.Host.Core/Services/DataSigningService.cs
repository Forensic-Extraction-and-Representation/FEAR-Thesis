using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace FEAR.Host.Core.Services
{
    public class DataSigningService
    {
        private readonly RSA _rsaProvider;
        private readonly IConfiguration _configuration;
        public DataSigningService(IConfiguration configuration)
        {
            _configuration = configuration;

            _rsaProvider = RSA.Create();
            _rsaProvider.ImportRSAPrivateKey(Convert.FromBase64String(_configuration["ChatInference:SigningKey"]), out _);
        }

        public string GenerateSignature(Stream dataSource)
        {
            int originalPosition = (int)dataSource.Position; // Save the current position
            dataSource.Position = 0; // Ensure the stream is at the beginning

            // Compute the hash of the data
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(dataSource);

            dataSource.Position = originalPosition; // Restore the original position of the stream
            // Sign the hash using RSA
            var signatureBytes = _rsaProvider.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            // Return the signature as a Base64 string
            return Convert.ToBase64String(signatureBytes);
        }

        public string GenerateSignature(string data)
        {
            // Compute the hash of the data
            using var sha256 = SHA256.Create();
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var hash = sha256.ComputeHash(dataBytes);
            // Sign the hash using RSA
            var signatureBytes = _rsaProvider.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            // Return the signature as a Base64 string
            return Convert.ToBase64String(signatureBytes);
        }

        public bool ValidateSignature(string sig, Stream dataSource)
        {
            int originalPosition = (int)dataSource.Position; // Save the current position
            dataSource.Position = 0; // Ensure the stream is at the beginning
            
            // Convert the signature from Base64 string to byte array
            var signatureBytes = Convert.FromBase64String(sig);
            // Compute the hash of the data
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(dataSource);

            dataSource.Position = originalPosition; // Restore the original position of the stream

            // Verify the signature using RSA
            return _rsaProvider.VerifyHash(hash, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public bool ValidateSignature(string signature, string data)
        {
            // Convert the signature from Base64 string to byte array
            var signatureBytes = Convert.FromBase64String(signature);
            // Compute the hash of the data
            using var sha256 = SHA256.Create();
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var hash = sha256.ComputeHash(dataBytes);
            // Verify the signature using RSA
            return _rsaProvider.VerifyHash(hash, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
