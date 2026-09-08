using FEAR.Blazor.Shared.Components.ConfigurationEditor.Model;
using FEAR.Blazor.Shared.Services;
using FEAR.Domain.Model;
using FEAR.Domain.Model.InvestigationStore;
using FEAR.Host.Domain.Api.Investigation;
using System.Text.Json;

namespace FEAR.Admin.Services.Management
{
    public class InvestigationService : BaseService
    {
        public InvestigationService(HttpFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public async Task<QueryInvestigations.Response> QueryInvestigations(QueryInvestigations.Request request)
        {
            return await PostAsync<QueryInvestigations.Request, QueryInvestigations.Response>("v1.0/Management/Investigation/QueryInvestigations", request);
        }

        public async Task SaveInvestigation(InvestigationInfo investigationInfo, CaseConfiguration configuration)
        {
            var request = new UpdateInvestigation.Request
            {
                InvestigationId = investigationInfo.InvestigationId,
                Name = investigationInfo.Name,
                Description = investigationInfo.Description,
                CaseNumber = investigationInfo.CaseNumber,
                CaseDate = investigationInfo.CaseDate,
                CaseStatus = investigationInfo.CaseStatus,
                CaseType = investigationInfo.CaseType,
                Namespace = investigationInfo.Namespace,
                NamespaceAbbrev = investigationInfo.NamespaceAbbrev,
                Configuration = configuration
            };

            request.Configuration.InvestigationName = request.Name;

            await PostAsync<UpdateInvestigation.Request, UpdateInvestigation.Response>("v1.0/Management/Investigation/SaveInvestigation", request);
        }

        public async Task RestartInvestigationGraphService(string investigationName)
        {
            await GetAsync<object>($"v1.0/HostedApi/RestartInvestigationGraphService/{investigationName}");
        }

        public async Task StopInvestigationGraphService(string investigationName)
        {
            await GetAsync<object>($"v1.0/HostedApi/StopInvestigationGraphService/{investigationName}");
        }

        public async Task StartInvestigationGraphService(string investigationName)
        {
            await GetAsync<object>($"v1.0/HostedApi/StartInvestigationGraphService/{investigationName}");
        }

        public async Task<string> GetInvestigationGraphServiceState(string investigationName)
        {
            var result = await GetAsync<GraphServiceStateResponse>($"v1.0/HostedApi/GetInvestigationGraphServiceState/{investigationName}");
            return result?.State ?? "Stopped";
        }

        public async Task<QueryInvestigationTelemetry.Response> QueryInvestigationTelemetry(QueryInvestigationTelemetry.Request request)
        {
            return await PostAsync<QueryInvestigationTelemetry.Request, QueryInvestigationTelemetry.Response>(
                $"v1.0/HostedApi/QueryInvestigationTelemetry/{request.InvestigationName}", request);
        }

        public async Task<List<ConfigurationField>> GetConfigurationFields()
        {
            return await GetAsync<List<ConfigurationField>>("v1.0/Management/Investigation/GetConfigurationFields");
        }

        public async Task DeleteInvestigation(InvestigationInfo investigationInfo)
        {
            await GetAsync<object>($"v1.0/Management/Investigation/DeleteInvestigation/{investigationInfo.InvestigationId}");
        }

        public async Task<FormRoot> GetInvestigationConfigurationEditorRoot()
        {
            var formRoot = await GetUnauthenticatedAsync<FormRoot>("v1.0/Management/Investigation/GetInvestigationConfigurationEditorRoot");
            formRoot.Sync();
            return formRoot;
        }

        public async Task<CaseConfiguration> GetInvestigationConfiguration(InvestigationInfo investigation)
        {
            var config = await GetAsync<CaseConfiguration>($"v1.0/Management/Investigation/GetInvestigationConfiguration/{investigation.InvestigationId}");
            config.TranslateDictionaryObjects();
            return config;
        }

        public async Task<IEnumerable<InvestigationQuery>> GetInvestigationQueries(InvestigationInfo investigation)
        {
            var queries = await GetAsync<IEnumerable<InvestigationQuery>>($"v1.0/Management/Investigation/GetInvestigationQueries/{investigation.InvestigationId}");
            return queries;
        }

        public async Task DeleteInvestigationQuery(InvestigationQuery query)
        {
            await PostAsync<InvestigationQuery>($"v1.0/Management/Investigation/DeleteInvestigationQuery", query);
        }

        public async Task<InvestigationQuery> SaveInvestigationQuery(InvestigationQuery query)
        {
            var response = await PostAsync<InvestigationQuery, InvestigationQuery>("v1.0/Management/Investigation/SaveInvestigationQuery", query);
            return response;
        }

        public async Task<ExportCase.Response> ExportCase(ExportCase.Request request)
        {
            return await PostAsync<ExportCase.Request, ExportCase.Response>("v1.0/Management/Investigation/ExportCase", request);
        }

        public async Task<ImportCase.Response> ImportCase(ImportCase.Request request)
        {
            return await PostAsync<ImportCase.Request, ImportCase.Response>("v1.0/Management/Investigation/ImportCase", request);
        }

        public async Task<GetInvestigationOntology.Response> GetInvestigationOntology(InvestigationInfo investigation)
        {
            return await GetAsync<GetInvestigationOntology.Response>($"v1.0/HostedApi/GetInvestigationOntology/{investigation.Name}");
        }

        public async Task<GetInvestigationGraphStatistics.Response> GetInvestigationGraphStatistics(InvestigationInfo investigation)
        {
            return await GetAsync<GetInvestigationGraphStatistics.Response>($"v1.0/HostedApi/GetInvestigationGraphStatistics/{investigation.Name}");
        }

        public async Task<GetInvestigationGraphData.Response> GetInvestigationGraphData(InvestigationInfo investigation)
        {
            return await GetAsync<GetInvestigationGraphData.Response>($"v1.0/HostedApi/GetInvestigationGraphData/{investigation.Name}");
        }

        private class GraphServiceStateResponse
        {
            public string State { get; set; }
        }
    }
}
