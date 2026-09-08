using FEAR.Domain.Model;
using Microsoft.Extensions.Configuration;

namespace FEAR.Host.Core.Investigation.Configuration
{
    public abstract class BaseInvestigationConfigurationProvider : IInvestigationConfigurationProvider
    {
        protected IServiceProvider _serviceProvider;

        public abstract CaseConfiguration CreateInvestigationConfiguration(string caseName);
        public abstract List<string> GetAllInvestigationNames();
        public abstract CaseConfiguration GetInvestigationConfiguration(string caseName);

        public virtual void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public abstract bool InvestigationConfigurationExists(string caseName);
        public abstract bool SaveInvestigationConfiguration(string caseName, CaseConfiguration configuration);
    }
}
