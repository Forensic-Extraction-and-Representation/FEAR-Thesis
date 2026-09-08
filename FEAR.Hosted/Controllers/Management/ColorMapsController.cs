using FEAR.Api.Controllers;
using FEAR.Domain;
using FEAR.Domain.Dto.ColorMap;
using FEAR.Host.Core.Database;
using FEAR.Host.Core.Identity;
using FEAR.Host.Domain.Api.Management.ColorMaps;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace FEAR.Hosted.Controllers.Management
{
    [ApiController]
    [Route("v1.0/Management/[controller]/[action]")]
    public class ColorMapsController : AuthenticatedControllerBase
    {
        private readonly InvestigationManagementDbContext _db;
        public ColorMapsController(IIdentityProvider identityProvider, IPermissionProvider permissionProvider, InvestigationManagementDbContext db)
            : base(identityProvider, permissionProvider)
        {
            _db = db;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult QueryColorMapSets([FromBody] QueryColorMapSets.Request request)
        {
            // IF the discriminator is User, set the entity to the current user
            // otherwise, check if the user has access to the investigation
            // If it's none, they need to have admin rights
            var q = _db.ColorMaps.AsQueryable();
            q = q.Where(s => s.EntityDiscriminator == request.Scope);
            if (request.EntityId.HasValue)
                q = q.Where(s => s.EntityId == request.EntityId.ToString());
            if (!string.IsNullOrWhiteSpace(request.NameFilter))
                q = q.Where(s => s.Name.Contains(request.NameFilter));

            var results = q.ToList();
            return Ok(new QueryColorMapSets.Response { Results = results, ReturnedResults = results.Count, TotalResults = results.Count });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult UpsertColorMapSet([FromBody] UpsertColorMapSet.Request request)
        {
            var set = request.Set;

            // IF the discriminator is User, set the entity to the current user
            // otherwise, check if the user has access to the investigation
            // If it's none, they need to have admin rights

            if (set != null)
            {
                if (set.EntityDiscriminator == FEAR.Domain.Model.ColorMap.ColorMapEntityDiscriminator.User)
                {
                    set.EntityId = CurrentUser.Value.UserId.ToString();
                }
                else if (set.EntityDiscriminator == FEAR.Domain.Model.ColorMap.ColorMapEntityDiscriminator.None)
                {
                    var manageAction = CanPerformSystemAction(ActionDefinitions.System.ColorMap.Manage);
                    if (!manageAction.IsApproved)
                        return Forbid();
                }
                else if (set.EntityDiscriminator == FEAR.Domain.Model.ColorMap.ColorMapEntityDiscriminator.Investigation)
                {
                    if (Guid.TryParse(set.EntityId, out var investigationId))
                    {
                        var access = CanPerformInvestigationAction(ActionDefinitions.System.ColorMap.Manage, investigationId);
                        if (!access.IsApproved)
                            return Forbid();
                    }
                    else
                    {
                        return Forbid();
                    }
                }

                if (set.ColorMapSetId == Guid.Empty)
                {
                    set.ColorMapSetId = Guid.NewGuid();

                    _db.ColorMaps.Add(new ColorMapSet
                    {
                        ColorMapSetId = set.ColorMapSetId,
                        Name = set.Name,
                        Description = set.Description,
                        EntityDiscriminator = set.EntityDiscriminator,
                        EntityId = set.EntityId,
                        ColorMaps = set.ColorMaps?.ToList() ?? new List<FEAR.Domain.Model.ColorMap.ColorMap>()
                    });
                }
                else
                {
                    var existing = _db.ColorMaps.FirstOrDefault(x => x.ColorMapSetId == set.ColorMapSetId);
                    if (existing == null) return NotFound();
                    _db.Entry(existing).CurrentValues.SetValues(set);
                }
                _db.SaveChanges();
                return Ok(new UpsertColorMapSet.Response { Success = true, ColorMapSetId = set.ColorMapSetId });
            }
            return BadRequest("No ColorMapSet provided.");
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult DeleteColorMapSet([FromBody] DeleteColorMapSet.Request request)
        {
            var existing = _db.ColorMaps.FirstOrDefault(x => x.ColorMapSetId == request.ColorMapSetId);

            if (existing == null) return Forbid();

            if (existing.EntityDiscriminator == FEAR.Domain.Model.ColorMap.ColorMapEntityDiscriminator.User)
            {
                if (existing.EntityId != CurrentUser.Value.UserId.ToString())
                    return Forbid();
            }

            else if (existing.EntityDiscriminator == FEAR.Domain.Model.ColorMap.ColorMapEntityDiscriminator.None)
            {
                var manageAction = CanPerformSystemAction(ActionDefinitions.System.ColorMap.Manage);
                if(!manageAction.IsApproved)
                    return Forbid();
            }

            else if (existing.EntityDiscriminator == FEAR.Domain.Model.ColorMap.ColorMapEntityDiscriminator.Investigation)
            {
                var investigationId = Guid.Parse(existing.EntityId);
                var access = CanPerformInvestigationAction(ActionDefinitions.System.ColorMap.Manage, investigationId);
                if (!access.IsApproved)
                    return Forbid();
            }

            _db.ColorMaps.Remove(existing);
            _db.SaveChanges();
            return Ok(new DeleteColorMapSet.Response { Success = true });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult PromoteColorMapSet([FromBody] PromoteColorMapSet.Request request)
        {
            var clone = new ColorMapSet
            {
                ColorMapSetId = Guid.NewGuid(),
                Name = request.ColorMapSet.Name,
                EntityDiscriminator = FEAR.Domain.Model.ColorMap.ColorMapEntityDiscriminator.User,
                EntityId = CurrentUser.Value.UserId.ToString(),
                ColorMaps = request.ColorMapSet.ColorMaps?.ToList() ?? new List<FEAR.Domain.Model.ColorMap.ColorMap>()
            };

            _db.ColorMaps.Add(clone);
            _db.SaveChanges();
            return Ok(new PromoteColorMapSet.Response { Success = true, NewColorMapSetId = clone.ColorMapSetId });
        }
    }
}
