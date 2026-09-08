using FEAR.Host.Core.ProtectedEncryptionKey;
using FEAR.Security.Providers;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace FEAR.Security
{
    public static class SecurityManagementExtensions
    {
        public static SecurityManagementBuilder AddWebManagement(this IServiceCollection services, Action<SecurityManagementBuilderOptions> options)
        {
            return new SecurityManagementBuilder(services).Configure(options);
        }

        public static IApplicationBuilder AddWebManagementAssets(this IApplicationBuilder host)
        {
            host.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new EmbeddedFileProvider(
            assembly: Assembly.Load(new AssemblyName("FEAR.Security")),
            baseNamespace: "FEAR.Security")
            });
            // */
            return host;
        }
    }
}
