using FEAR.Domain.Model.ColorMap;
using FEAR.Host.Domain.Api.Management.ColorMaps;
using FEAR.WASM.Components.Graph;
using FEAR.WASM.Domain;
using FEAR.WASM.Model;
using FEAR.WASM.Services;
using System.Net.Http.Json;

namespace FEAR.WASM.Providers
{
    public interface IColorMapProvider
    {
        Task InitializeAsync(Guid? investigationId);
        // Resolve final styles used by GraphContainer
        ColorMap Resolve(string predicateUri, string subjectUri, string? objectValue);
        string ReduceUri(string uri);

        // CRUD helpers
        Task<IReadOnlyList<ColorMapSet>> QueryAsync(ColorMapEntityDiscriminator scope, Guid? entityId = null);
        Task<Guid> UpsertAsync(ColorMapSet set);
        Task DeleteAsync(Guid colorMapSetId);
        Task<Guid> PromoteToUserAsync(ColorMapSet colorMapSet);

        // Session controls
        void AddSessionMap(ColorMapSet map);
        void ClearSessionMaps();

        // Inspection helpers
        IReadOnlyList<ColorMapSet> GetActiveMaps();
        IReadOnlyList<ColorMapSet> GetScopeMaps(ColorMapEntityDiscriminator scope);
        IReadOnlyList<ColorMapSet> GetSessionMaps();
        IEnumerable<LegendItem> GetLegentItems();
    }

    public class ColorMapProvider : IColorMapProvider
    {
        private readonly WASMInternalHttpFactory _httpFactory;

        // Scopes cached in memory
        private IEnumerable<ColorMapSet> _system = new List<ColorMapSet>();
        private IEnumerable<ColorMapSet> _investigation = new List<ColorMapSet>();
        private IEnumerable<ColorMapSet> _user = new List<ColorMapSet>();
        private IList<ColorMapSet> _session = new List<ColorMapSet>();

        private LegendStyleCache _legendCache = new([]);
        private ValueStyleCache _valueCache = new([]);

        public ColorMapProvider(WASMInternalHttpFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task InitializeAsync(Guid? investigationId)
        {
            _system = await LoadScopeAsync(ColorMapEntityDiscriminator.None, null);
            _investigation = investigationId.HasValue ? await LoadScopeAsync(ColorMapEntityDiscriminator.Investigation, investigationId) : new List<ColorMapSet>();
            _user = await LoadScopeAsync(ColorMapEntityDiscriminator.User, null);

            RebuildCaches();
        }

        private void RebuildCaches()
        {
            var merged = _session.Concat(_user).Concat(_investigation).Concat(_system).ToList();
            _legendCache = new LegendStyleCache(merged.GetAllColorMaps());
            _valueCache = new ValueStyleCache(merged.GetAllColorMaps());
        }

        public void AddSessionMap(ColorMapSet map)
        {
            if(map == null || map.ColorMapSetId == Guid.Empty)
                return;

            if (_session == null)
                _session = new List<ColorMapSet>();

            if (!_session.Any(m => m.ColorMapSetId == map.ColorMapSetId))
                _session.Add(map);
            else
            {
                var idx = _session.ToList().FindIndex(m => m.ColorMapSetId == map.ColorMapSetId);
                if (idx >= 0)
                    _session[idx] = map;
            }

            RebuildCaches();
        }

        public void ClearSessionMaps()
        {
            _session.Clear();
            RebuildCaches();
        }

        public IEnumerable<LegendItem> GetLegentItems()
            => GetActiveMaps().GetLegendItems();

        public string ReduceUri(string uri)
        {
            if (Uri.TryCreate(uri, UriKind.Absolute, out var u))
                return _legendCache.ReduceUri(u) ?? uri;
            return uri;
        }

        public ColorMap Resolve(string predicateUri, string subjectUri, string? objectValue)
        {
            // Try legend (predicate, subject) then value (object)
            ColorMap? hit = null;
            if (Uri.TryCreate(predicateUri, UriKind.Absolute, out var p))
                hit = _legendCache.GetColor(p);
            else if ( Uri.TryCreate(subjectUri, UriKind.Absolute, out var s))
                hit = _legendCache.GetColor(s);
            else if (!string.IsNullOrWhiteSpace(objectValue))
                hit = _valueCache.GetValueColor(objectValue);

            return hit ?? LegendStyleCache.DefaultColorMap;
        }

        private async Task<IEnumerable<ColorMapSet>> LoadScopeAsync(ColorMapEntityDiscriminator scope, Guid? entityId)
        {
            var client = await _httpFactory.CreateHttpClientAsync(true);
            var req = new QueryColorMapSets.Request { Scope = scope, EntityId = entityId };
            var resp = await client.PostAsJsonAsync("v1.0/Management/ColorMaps/QueryColorMapSets", req);
            resp.EnsureSuccessStatusCode();
            var data = await resp.Content.ReadFromJsonAsync<QueryColorMapSets.Response>();
            // Map DTO sets (each set contains Model.ColorMap list) into WASM ColorMap model
            var list = new List<ColorMap>();
            return data?.Results ?? Enumerable.Empty<ColorMapSet>();
        }

        public async Task<IReadOnlyList<ColorMapSet>> QueryAsync(ColorMapEntityDiscriminator scope, Guid? entityId = null)
        {
            var client = await _httpFactory.CreateHttpClientAsync(true);
            var req = new QueryColorMapSets.Request { Scope = scope, EntityId = entityId };
            var resp = await client.PostAsJsonAsync("v1.0/Management/ColorMaps/QueryColorMapSets", req);
            resp.EnsureSuccessStatusCode();
            var data = await resp.Content.ReadFromJsonAsync<QueryColorMapSets.Response>();
            return data?.Results?.ToList() ?? new List<ColorMapSet>();
        }

        public async Task<Guid> UpsertAsync(ColorMapSet set)
        {
            var client = await _httpFactory.CreateHttpClientAsync(true);
            var resp = await client.PostAsJsonAsync("v1.0/Management/ColorMaps/UpsertColorMapSet", new UpsertColorMapSet.Request { Set = set });
            resp.EnsureSuccessStatusCode();
            var data = await resp.Content.ReadFromJsonAsync<UpsertColorMapSet.Response>();
            return data.ColorMapSetId;
        }

        public async Task DeleteAsync(Guid colorMapSetId)
        {
            var client = await _httpFactory.CreateHttpClientAsync(true);
            var resp = await client.PostAsJsonAsync("v1.0/Management/ColorMaps/DeleteColorMapSet", new DeleteColorMapSet.Request { ColorMapSetId = colorMapSetId });
            resp.EnsureSuccessStatusCode();
        }

        public async Task<Guid> PromoteToUserAsync(ColorMapSet colorMapSet)
        {
            var client = await _httpFactory.CreateHttpClientAsync(true);
            var resp = await client.PostAsJsonAsync("v1.0/Management/ColorMaps/PromoteColorMapSet", new PromoteColorMapSet.Request { ColorMapSet = colorMapSet });
            resp.EnsureSuccessStatusCode();
            var data = await resp.Content.ReadFromJsonAsync<PromoteColorMapSet.Response>();
            return data.NewColorMapSetId;
        }

        public IReadOnlyList<ColorMapSet> GetActiveMaps()
            => _session.Concat(_user).Concat(_investigation).Concat(_system).ToList();

        public IReadOnlyList<ColorMapSet> GetScopeMaps(ColorMapEntityDiscriminator scope)
            => scope switch
            {
                ColorMapEntityDiscriminator.User => _user.ToList(),
                ColorMapEntityDiscriminator.Investigation => _investigation.ToList(),
                ColorMapEntityDiscriminator.None => _system.ToList(),
                _ => new List<ColorMapSet>()
            };

        public IReadOnlyList<ColorMapSet> GetSessionMaps() => _session.ToList();
    }
}
