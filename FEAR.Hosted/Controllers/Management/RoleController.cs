using AutoMapper;
using FEAR.Api.Controllers;
using FEAR.Domain;
using FEAR.Host.Core.Database;
using FEAR.Host.Core.Identity;
using FEAR.Host.Domain.Api.Management.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;

namespace FEAR.Hosted.Controllers.Management
{
    [ApiController]
    [Route("v1.0/Management/[controller]/[action]")]
    public class RoleController : AuthenticatedControllerBase
    {
        private readonly IdentityDbContext _db;
        private readonly IMapper _mapper;

        public RoleController(IIdentityProvider identityProvider, IPermissionProvider permissionProvider, IdentityDbContext db, IMapper mapper)
            : base(identityProvider, permissionProvider)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult QueryRoles([FromBody] QueryRoles.Request request)
        {
            var perm = CanPerformSystemAction(ActionDefinitions.System.Role.List);
            if (!perm.IsApproved) return Unauthorized();

            var q = _db.Roles.Include(r => r.Permissions).AsQueryable();
            if (!string.IsNullOrWhiteSpace(request?.NameFilter))
                q = q.Where(r => r.Name.Contains(request.NameFilter));

            var results = q.ToList().Select(r => new FEAR.Domain.Model.Authentication.Role
            {
                RoleId = r.RoleId,
                Name = r.Name,
                Permissions = r.Permissions.Select(p => _mapper.Map<FEAR.Domain.Model.Authentication.PermissionSet>(p)).ToList()
            }).ToList();
            return Ok(new QueryRoles.Response { Results = results, ReturnedResults = results.Count, TotalResults = results.Count });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult CreateRole([FromBody] CreateRole.Request request)
        {
            var perm = CanPerformSystemAction(ActionDefinitions.System.Role.Create);
            if (!perm.IsApproved) return Unauthorized();

            var role = new FEAR.Domain.Dto.Authentication.Role { Name = request.Name };
            if (request.PermissionSetIds?.Any() == true)
            {
                role.Permissions = _db.PermissionSets.Where(p => request.PermissionSetIds.Contains(p.PermissionSetId)).ToList();
            }
            _db.Roles.Add(role);
            _db.SaveChanges();
            return Ok(new CreateRole.Response { RoleId = role.RoleId, Success = true });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult UpdateRole([FromBody] UpdateRole.Request request)
        {
            var perm = CanPerformSystemAction(ActionDefinitions.System.Role.Update);
            if (!perm.IsApproved) return Unauthorized();

            var role = _db.Roles.Include(r => r.Permissions).FirstOrDefault(r => r.RoleId == request.RoleId);
            if (role == null) return NotFound();

            role.Name = request.Name;
            role.Permissions.Clear();
            if (request.PermissionSetIds?.Any() == true)
            {
                var newPerms = _db.PermissionSets.Where(p => request.PermissionSetIds.Contains(p.PermissionSetId)).ToList();
                foreach (var p in newPerms) role.Permissions.Add(p);
            }
            _db.SaveChanges();
            return Ok(new UpdateRole.Response { Success = true });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult DeleteRole([FromBody] DeleteRole.Request request)
        {
            var perm = CanPerformSystemAction(ActionDefinitions.System.Role.Delete);
            if (!perm.IsApproved) return Unauthorized();

            var role = _db.Roles.FirstOrDefault(r => r.RoleId == request.RoleId);
            if (role == null) return NotFound();

            _db.Roles.Remove(role);
            _db.SaveChanges();
            return Ok(new DeleteRole.Response { Success = true });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult QueryPermissionSets([FromBody] QueryPermissionSets.Request request)
        {
            var perm = CanPerformSystemAction(ActionDefinitions.System.PermissionSet.List);
            if (!perm.IsApproved) return Unauthorized();

            var q = _db.PermissionSets
                .Include(p => p.ApprovedActions)
                .Include(p => p.DeniedActions)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request?.NameFilter))
                q = q.Where(p => p.Name.Contains(request.NameFilter));

            var results = q.ToList().Select(a => new FEAR.Domain.Model.Authentication.PermissionSet
            {
                PermissionSetId = a.PermissionSetId,
                Name = a.Name,
                ApprovedActions = a.ApprovedActions.Select(act => _mapper.Map<FEAR.Domain.Model.Authentication.SystemAction>(act)).ToList(),
                DeniedActions = a.DeniedActions.Select(act => _mapper.Map<FEAR.Domain.Model.Authentication.SystemAction>(act)).ToList()
            }).ToList();
            return Ok(new QueryPermissionSets.Response { Results = results, ReturnedResults = results.Count, TotalResults = results.Count });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult CreatePermissionSet([FromBody] CreatePermissionSet.Request request)
        {
            var perm = CanPerformSystemAction(ActionDefinitions.System.PermissionSet.Create);
            if (!perm.IsApproved) return Unauthorized();

            var ps = new FEAR.Domain.Dto.Authentication.PermissionSet { Name = request.Name };
            if (request.ApprovedActionIds?.Any() == true)
                ps.ApprovedActions = _db.SystemActions.Where(a => request.ApprovedActionIds.Contains(a.SystemActionId)).ToList();
            if (request.DeniedActionIds?.Any() == true)
                ps.DeniedActions = _db.SystemActions.Where(a => request.DeniedActionIds.Contains(a.SystemActionId)).ToList();

            _db.PermissionSets.Add(ps);
            _db.SaveChanges();
            return Ok(new CreatePermissionSet.Response { PermissionSetId = ps.PermissionSetId, Success = true });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult UpdatePermissionSet([FromBody] UpdatePermissionSet.Request request)
        {
            var perm = CanPerformSystemAction(ActionDefinitions.System.PermissionSet.Update);
            if (!perm.IsApproved) return Unauthorized();

            var ps = _db.PermissionSets
                .Include(p => p.ApprovedActions)
                .Include(p => p.DeniedActions)
                .FirstOrDefault(p => p.PermissionSetId == request.PermissionSetId);
            if (ps == null) return NotFound();

            ps.Name = request.Name;
            ps.ApprovedActions.Clear();
            ps.DeniedActions.Clear();

            if (request.ApprovedActionIds?.Any() == true)
            {
                var acts = _db.SystemActions.Where(a => request.ApprovedActionIds.Contains(a.SystemActionId)).ToList();
                foreach (var a in acts) ps.ApprovedActions.Add(a);
            }
            if (request.DeniedActionIds?.Any() == true)
            {
                var acts = _db.SystemActions.Where(a => request.DeniedActionIds.Contains(a.SystemActionId)).ToList();
                foreach (var a in acts) ps.DeniedActions.Add(a);
            }

            _db.SaveChanges();
            return Ok(new UpdatePermissionSet.Response { Success = true });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult DeletePermissionSet([FromBody] DeletePermissionSet.Request request)
        {
            var perm = CanPerformSystemAction(ActionDefinitions.System.PermissionSet.Delete);
            if (!perm.IsApproved) return Unauthorized();

            var ps = _db.PermissionSets.FirstOrDefault(p => p.PermissionSetId == request.PermissionSetId);
            if (ps == null) return NotFound();

            _db.PermissionSets.Remove(ps);
            _db.SaveChanges();
            return Ok(new DeletePermissionSet.Response { Success = true });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult QuerySystemActions([FromBody] QuerySystemActions.Request request)
        {
            var perm = CanPerformSystemAction(ActionDefinitions.System.PermissionSet.List);
            if (!perm.IsApproved) return Unauthorized();

            var q = _db.SystemActions.AsQueryable();
            if (!string.IsNullOrWhiteSpace(request?.NameFilter))
                q = q.Where(a => a.Name.Contains(request.NameFilter));

            var results = q.ToList().Select(a => _mapper.Map<FEAR.Domain.Model.Authentication.SystemAction>(a)).ToList();
            return Ok(new QuerySystemActions.Response { Results = results, ReturnedResults = results.Count, TotalResults = results.Count });
        }
    }
}
