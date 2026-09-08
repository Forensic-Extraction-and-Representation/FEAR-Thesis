using FEAR.Domain.Model.InvestigationStore;
using FEAR.Host.Domain.Api.Investigation;
using FEAR.Hosted.Domain.Api.Investigation;
using FEAR.WASM.Model;
using System.Net.Http.Json;
using System.Text.Json;

namespace FEAR.WASM.Services
{
    public class InvestigationService
    {
        private readonly WebUIConfiguration _configuration;
        private readonly WASMInternalHttpFactory _httpClientFactory;

        public InvestigationService(WASMInternalHttpFactory httpClientFactory, WebUIConfiguration configuration)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PostArtifactResponse> PostArtifactAsync(string jsonData)
        {
            var client = await _httpClientFactory.CreateHttpClientAsync();
            HttpContent content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"v1.0/HostedApi/PostArtifacts/{_configuration.InvestigationName}", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PostArtifactResponse>();
        }

        public async Task<Tuple<bool, string>> GetArtifactWorkItemAsync(Guid executionId)
        {
            var client = await _httpClientFactory.CreateHttpClientAsync();
            var response = await client.GetAsync($"v1.0/HostedApi/GetArtifactWorkItemResult/{_configuration.InvestigationName}/{executionId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new Tuple<bool, string>(false, string.Empty);
            else
            {
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsStringAsync();
                return new Tuple<bool, string>(true, data);
            }
        }

        public async Task<InvestigationInfo> GetInvestigationByName(string? investigationName)
        {
            var client = await _httpClientFactory.CreateHttpClientAsync();
            var response = await client.GetAsync($"v1.0/HostedApi/GetInvestigationByName/{investigationName}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            else
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<InvestigationInfo>();
            }
        }

        public async Task<List<InvestigationInfo>> GetAllInvestigationsAsync()
        {
            var client = await _httpClientFactory.CreateHttpClientAsync();
            HttpContent content = new StringContent(JsonSerializer.Serialize(new QueryInvestigations.Request()), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"v1.0/Management/Investigation/QueryInvestigations", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<InvestigationInfo>>();
        }
    }
}