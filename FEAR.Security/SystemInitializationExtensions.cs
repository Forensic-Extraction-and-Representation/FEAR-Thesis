using FEAR.Host.Core.ProtectedEncryptionKey;
using FEAR.Security.Providers;

namespace FEAR.Security
{
    public static class SystemInitializationExtensions
    {
        public static ProtectedEncryptionKeyBuilder AddProtectedEncryptionKey(this IServiceCollection services, Action<ProtectedEncryptionKeyBuilderOptions> options)
        {
            return new ProtectedEncryptionKeyBuilder(services).Configure(options);
        }
    }
}
