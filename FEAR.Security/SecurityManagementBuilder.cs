using FEAR.Host.Core.ProtectedEncryptionKey;
using FEAR.Security.Controllers;
using System.Reflection;

namespace FEAR.Security.Providers
{
    public class SecurityManagementBuilder
    {
        private IServiceCollection _services;
        public SecurityManagementBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public SecurityManagementBuilder Configure(Action<SecurityManagementBuilderOptions> options)
        {
            var assembly = typeof(InitializationController).GetTypeInfo().Assembly;
            _services.AddMvc().AddApplicationPart(assembly).AddRazorRuntimeCompilation();

            var builderOptions = new SecurityManagementBuilderOptions();
            options(builderOptions);
            
            return this;
        }
    }
}
