using FEAR.Domain.Model;

namespace FEAR.Host.Core.Investigation.Configuration
{
    public interface IInvestigationConfigurationProvider
    {
        void Initialize(IServiceProvider serviceProvider);

        List<string> GetAllInvestigationNames();
        bool InvestigationConfigurationExists(string caseName);
        CaseConfiguration GetInvestigationConfiguration(string caseName);
        CaseConfiguration CreateInvestigationConfiguration(string caseName);
        bool SaveInvestigationConfiguration(string caseName, CaseConfiguration configuration);
    }
}
