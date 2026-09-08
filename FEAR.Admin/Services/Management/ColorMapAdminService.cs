using FEAR.Blazor.Shared.Services;
using FEAR.Domain.Model.ColorMap;
using FEAR.Host.Domain.Api.Management.ColorMaps;

namespace FEAR.Admin.Services.Management
{
    public class ColorMapAdminService : BaseService
    {
        public ColorMapAdminService(HttpFactory http) : base(http) { }

        public async Task<List<ColorMapSet>> QueryAsync(ColorMapEntityDiscriminator scope, Guid? entityId)
        {
            var resp = await PostAsync<QueryColorMapSets.Request, QueryColorMapSets.Response>(
                "v1.0/Management/ColorMaps/QueryColorMapSets",
                new QueryColorMapSets.Request { Scope = scope, EntityId = entityId });
            return resp.Results.ToList();
        }

        public async Task<Guid> UpsertAsync(ColorMapSet set)
        {
            var resp = await PostAsync<UpsertColorMapSet.Request, UpsertColorMapSet.Response>(
                "v1.0/Management/ColorMaps/UpsertColorMapSet",
                new UpsertColorMapSet.Request { Set = set });
            return resp.ColorMapSetId;
        }

        public async Task DeleteAsync(Guid id)
        {
            await PostAsync("v1.0/Management/ColorMaps/DeleteColorMapSet",
                new DeleteColorMapSet.Request { ColorMapSetId = id });
        }
    }
}
