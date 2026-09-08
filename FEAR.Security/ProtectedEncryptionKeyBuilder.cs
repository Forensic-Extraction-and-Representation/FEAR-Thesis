using FEAR.Host.Core.ProtectedEncryptionKey;
using FEAR.Security.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;

namespace FEAR.Security.Providers
{
    public class ProtectedEncryptionKeyBuilder
    {
        private IServiceCollection _services;

        private class SelectedControllersApplicationParts : ApplicationPart, IApplicationPartTypeProvider
        {
            public SelectedControllersApplicationParts()
            {
                Name = "Only allow selected controllers";
            }
            public SelectedControllersApplicationParts(Type[] types)
            {
                Types = types.Select(x => x.GetTypeInfo()).ToArray();
            }

            public override string Name { get; }

            public IEnumerable<TypeInfo> Types { get; }
        }

        public ProtectedEncryptionKeyBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public ProtectedEncryptionKeyBuilder Configure(Action<ProtectedEncryptionKeyBuilderOptions> options)
        {
            var builderOptions = new ProtectedEncryptionKeyBuilderOptions();
            options(builderOptions);

            if (builderOptions.Enabled)
            {
                var assembly = typeof(InitializationController).GetTypeInfo().Assembly;
                _services.AddMvc().AddApplicationPart(assembly);

                _services.AddSingleton<IProtectedEncryptionKeyProvider>(new ProtectedEncryptionKeyProvider(builderOptions));
            }

            return this;
        }
    }
}
