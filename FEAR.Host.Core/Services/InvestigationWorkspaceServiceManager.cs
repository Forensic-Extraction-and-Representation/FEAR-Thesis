using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FEAR.Host.Core.Services
{
    public enum GraphServiceState
    {
        Running,
        Stopped
    }

    public class InvestigationWorkspaceServiceManager
    {
        private readonly ILogger<InvestigationWorkspace> _logger;
        private readonly IConfiguration _systemConfiguration;
        private readonly InvestigationConfigurationService _configurationService;
        private Dictionary<string, InvestigationWorkspace> _caseGraphServices = new Dictionary<string, InvestigationWorkspace>();
        private object _lock = new object();
        private readonly IServiceProvider _serviceProvider;
        public InvestigationWorkspaceServiceManager(ILogger<InvestigationWorkspace> logger, IConfiguration systemConfiguration, InvestigationConfigurationService configurationService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _systemConfiguration = systemConfiguration;
            _configurationService = configurationService;
        }

        public InvestigationWorkspace CreateInvestigationGraphService(string caseName)
        {
            lock (_lock)
            {
                if (!_caseGraphServices.ContainsKey(caseName))
                {
                    var config = _configurationService.GetInvestigationConfiguration(caseName);
                    var autopsyCaseGraph = ActivatorUtilities.CreateInstance<InvestigationWorkspace>(_serviceProvider, config, _systemConfiguration);
                    _caseGraphServices.Add(caseName, autopsyCaseGraph);

                    autopsyCaseGraph.Initialize();
                }
            }

            return _caseGraphServices[caseName];
        }

        public void RestartInvestigationGraphService(string caseName)
        {
            lock (_lock)
            {
                if (_caseGraphServices.ContainsKey(caseName))
                {
                    _caseGraphServices[caseName].Shutdown();
                    _caseGraphServices.Remove(caseName);
                }
            }

            CreateInvestigationGraphService(caseName);
        }

        public void StopInvestigationGraphService(string caseName)
        {
            lock (_lock)
            {
                if (_caseGraphServices.ContainsKey(caseName))
                {
                    _caseGraphServices[caseName].Shutdown();
                    _caseGraphServices.Remove(caseName);
                }
            }
        }

        public InvestigationWorkspace StartInvestigationGraphService(string caseName)
        {
            return CreateInvestigationGraphService(caseName);
        }

        public GraphServiceState GetInvestigationGraphServiceState(string caseName)
        {
            lock (_lock)
            {
                return _caseGraphServices.ContainsKey(caseName)
                    ? GraphServiceState.Running
                    : GraphServiceState.Stopped;
            }
        }
    }
}
