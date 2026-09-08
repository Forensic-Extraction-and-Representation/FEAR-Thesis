using System.Net.Http.Json;
using System.Text.Json;

namespace FEAR.Blazor.Shared.Services
{
    public abstract class BaseService
    {
        protected readonly HttpFactory _httpClientFactory;

        public BaseService(HttpFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<TResult> GetUnauthenticatedAsync<TResult>(string uri, JsonSerializerOptions options = null)
        {
            return await GetAsync<TResult>(uri, false, options);
        }

        public async Task<TResult> GetAsync<TResult>(string uri, JsonSerializerOptions options = null)
        {
            return await GetAsync<TResult>(uri, true, options);
        }

        private async Task<TResult> GetAsync<TResult>(string uri, bool authenticated, JsonSerializerOptions options = null)
        {
            var client = await _httpClientFactory.CreateHttpClientAsync(authenticated);
            var response = await client.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and message: {await response.Content.ReadAsStringAsync()}");
            }

            return await response.Content.ReadFromJsonAsync<TResult>(options);
        }

        public async Task PostUnauthenticatedAsync<TRequest>(string uri, TRequest request)
        {
            await PostAsync(uri, request, false);
        }

        public async Task<TResult> PostUnauthenticatedAsync<TRequest, TResult>(string uri, TRequest request)
        {
            return await PostAsync<TRequest, TResult>(uri, request, false);
        }

        public async Task PostAsync<TRequest>(string uri, TRequest request)
        {
            await PostAsync(uri, request, true);
        }

        public async Task<TResult> PostAsync<TRequest, TResult>(string uri, TRequest request)
        {
            return await PostAsync<TRequest, TResult>(uri, request, true);
        }

        private async Task PostAsync<TRequest>(string uri, TRequest request, bool authenticated)
        {
            var client = await _httpClientFactory.CreateHttpClientAsync(authenticated);
            var response = await client.PostAsJsonAsync(uri, request);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and message: {await response.Content.ReadAsStringAsync()}");
            }
        }

        private async Task<TResult> PostAsync<TRequest, TResult>(string uri, TRequest request, bool authenticated)
        {
            var client = await _httpClientFactory.CreateHttpClientAsync(authenticated);
            var response = await client.PostAsJsonAsync(uri, request);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and message: {await response.Content.ReadAsStringAsync()}");
            }
            return await response.Content.ReadFromJsonAsync<TResult>();
        }
    }
}
