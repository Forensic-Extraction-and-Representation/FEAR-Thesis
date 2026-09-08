using FEAR.Domain.Agents.Attachments;
using FEAR.Domain.Agents.Querying;
using FEAR.WASM.Model;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace FEAR.WASM.Services
{
    public class QueryServiceState
    {
        public GraphAttachment CurrentQueryResult { get; set; }
        public bool DownloadButtonEnabled { get; set; }
    }

    public class QueryService
    {
        private readonly WebUIConfiguration _configuration;
        private readonly WASMInternalHttpFactory _httpClientFactory;
        private readonly IJSRuntime _js;

        public QueryServiceState State { get; } = new();

        public QueryService(WASMInternalHttpFactory httpClientFactory, WebUIConfiguration configuration, IJSRuntime js)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _js = js;
        }

        public async Task<GraphQueryRequest> BeginQuery(string query)
        {
            State.DownloadButtonEnabled = false;
            var client = await _httpClientFactory.CreateHttpClientAsync();
            var response = await client.PostAsJsonAsync($"v1.0/Query/BeginQuery/{_configuration.InvestigationName}", new GraphQueryRequest() { Query = query, QueryFormat = "application/sparql-query" });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GraphQueryRequest>();
        }

        public async Task<IEnumerable<FEAR.Domain.Dto.InvestigationStore.InvestigationQuery>> GetSharedQueriesAsync()
        {
            var client = await _httpClientFactory.CreateHttpClientAsync();
            var response = await client.GetFromJsonAsync<IEnumerable<FEAR.Domain.Dto.InvestigationStore.InvestigationQuery>>($"v1.0/Query/GetSharedQueries/{_configuration.InvestigationName}");

            return response;
        }

        public GraphAttachment GetCurrentQueryResult()
        {
            return State.CurrentQueryResult;
        }

        public async Task<GraphAttachment> GetQueryResult(GraphQueryRequest beginResponse)
        {
            try
            {
                var client = await _httpClientFactory.CreateHttpClientAsync();
                var response = await client.GetAsync($"v1.0/Query/EndQuery/{_configuration.InvestigationName}?queryId={beginResponse.QueryIdentifier}");
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadFromJsonAsync<GraphAttachment>();
                State.CurrentQueryResult = data;
                return data;
            }
            catch(Exception e)
            {

            }
            return null;
        }

        public async Task<List<SharedQuery>> GetSharedQueries()
        {
            var client = await _httpClientFactory.CreateHttpClientAsync();
            var response = await client.GetAsync($"v1.0/HostedApi/GetSharedQueries/{_configuration.InvestigationName}");
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<List<SharedQuery>>();
            // UI population logic should be handled in the Blazor component, not here.
            return data;
        }

        // This method should be implemented in the Blazor component using JS interop for graph rendering.
        public GraphResult ProcessQueryResponse(GraphAttachment queryResult)
        {
            if (queryResult.Format.Contains("application/sparql-results+json"))
            {
                SparqlJsonParser sparqlReader = new SparqlJsonParser();
                SparqlResultSet sparqlResults = new SparqlResultSet();
                sparqlReader.Load(sparqlResults, new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(queryResult.Result))));
                // Parse and process SPARQL JSON results as needed.
                // Graphing logic should be handled in the Blazor component.
                return new GraphResult { Graphable = false, ResultSet = sparqlResults };
            }
            else if (queryResult.Format.Contains("text/turtle"))
            {
                TurtleParser turtleParser = new TurtleParser();
                var graph = new VDS.RDF.Graph();
                turtleParser.Load(graph, new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(queryResult.Result))));
                
                // Graphing logic should be handled in the Blazor component.
                return new GraphResult { Graphable = true, Graph = graph };
            }
            else
            {
                throw new Exception($"Unsupported response type: {queryResult.Format}");
            }
        }

        public async Task DownloadRdf()
        {
            if (State.CurrentQueryResult is GraphAttachment queryData && queryData.Result.Length > 0)
                await _js.InvokeVoidAsync("saveAsFile", "query_results.ttl", queryData.Result);
        }
    }
}