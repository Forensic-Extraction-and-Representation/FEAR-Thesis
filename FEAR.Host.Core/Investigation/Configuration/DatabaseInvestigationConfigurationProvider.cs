using FEAR.Domain.Database;
using FEAR.Domain.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace FEAR.Host.Core.Investigation.Configuration
{
    public class DatabaseInvestigationConfigurationProvider : BaseInvestigationConfigurationProvider
    {
        private readonly string _connectionString;
        public string ConnectionString => _connectionString;
        private Lazy<IInvestigationManagementDbContext> _invMgmtDbContext = null;
        private IServiceScope _scope;

        protected IInvestigationManagementDbContext InvMgmtDbContext => _invMgmtDbContext.Value;
        public DatabaseInvestigationConfigurationProvider(Dictionary<string, string> configurationOptions)
        {
            _connectionString = configurationOptions["ConnectionString"];

            _invMgmtDbContext = new Lazy<IInvestigationManagementDbContext>(() =>
            {
                _scope = _serviceProvider.CreateScope();
                return _scope.ServiceProvider.GetRequiredService<IInvestigationManagementDbContext>();
            });
        }

        public override CaseConfiguration CreateInvestigationConfiguration(string invName)
        {
            // Will need to lookup defaults.
            throw new NotImplementedException();
        }

        public override List<string> GetAllInvestigationNames() => InvMgmtDbContext.GetInvestigations().Select(i => i.Name).ToList();

        public override CaseConfiguration GetInvestigationConfiguration(string invName)
        {
            var invConfig = InvMgmtDbContext.GetInvestigationConfigurationByName(invName);
            var config = invConfig?.Configuration;
            if (config != null)
            {
                return config;
            }
            else
            {
                throw new Exception($"Case configuration for {invName} does not exist");
            }
        }

        public override bool InvestigationConfigurationExists(string inveName) => InvMgmtDbContext.GetInvestigationByName(inveName) != null;

        public override bool SaveInvestigationConfiguration(string invName, CaseConfiguration configuration)
        {
            var dbInvestigation = InvMgmtDbContext.GetInvestigationByName(invName);

            if ((dbInvestigation?.InvestigationId ?? Guid.Empty) != Guid.Empty)
            {
                var invConfig = InvMgmtDbContext.GetInvestigationConfigurationByName(invName);
                if (invConfig != null)
                {
                    invConfig.Configuration = configuration;

                    InvMgmtDbContext.SaveInvestigationConfiguration(invConfig);
                    return true;
                }
                else
                {
                    InvMgmtDbContext.SaveInvestigationConfiguration(new FEAR.Domain.Dto.InvestigationStore.InvestigationConfiguration()
                    {
                        InvestigationId = dbInvestigation.InvestigationId,
                        Configuration = configuration,
                        HostedUri = String.Empty,
                    });

                    return true;
                }
            }
            else
            {
                throw new Exception($"Investigation {invName} does not exist. Create the investigation before saving the configuration.");
            }

        }
    }
}
