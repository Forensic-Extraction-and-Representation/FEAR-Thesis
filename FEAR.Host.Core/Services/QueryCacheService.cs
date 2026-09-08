using FEAR.Domain.Agents.Querying;
using Jitbit.Utils;

namespace FEAR.Host.Core.Services
{
    public class QueryCacheService
    {
        private Dictionary<string, FastCache<string, GraphQueryAction>> queryCacheForCaseNames = new Dictionary<string, FastCache<string, GraphQueryAction>>();

        public QueryCacheService() { }

        public GraphQueryAction GetQueryStream(string caseName, string queryId)
        {
            if (!queryCacheForCaseNames.TryGetValue(caseName, out var queryCache))
            {
                return null; // Or throw an exception if preferred
            }

            if (queryCache.TryGet(queryId, out var request))
            {
                queryCache.Remove(queryId); // Remove the stream from cache after retrieval
                return request;
            }

            return null; // Or throw an exception if preferred
        }

        public void AddQueryResult(GraphQueryAction request)
        {
            if (!queryCacheForCaseNames.TryGetValue(request.CaseName, out var queryCache))
            {
                queryCache = new FastCache<string, GraphQueryAction>(); // Create a new cache for this case name
                queryCacheForCaseNames[request.CaseName] = queryCache; // Store it in the dictionary
            }

            queryCache.TryAdd(request.QueryIdentifier, request, TimeSpan.FromSeconds(60)); // Add the new stream to the cache
        }
    }
}
