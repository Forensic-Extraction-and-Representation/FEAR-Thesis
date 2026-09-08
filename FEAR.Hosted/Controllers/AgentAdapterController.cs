using FEAR.Domain.Agents;
using FEAR.Host.Core;
using FEAR.Host.Core.Agents;
using FEAR.Host.Core.Identity;
using FEAR.Host.Core.Services;
using Humanizer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FEAR.Hosted.Controllers
{
    [ApiController]
    [Route("v1.0/[controller]/[action]")]
    public partial class AgentAdapterController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Logger for diagnostic and audit purposes.
        /// </summary>
        private readonly ILogger<AgentAdapterController> _logger;

        private readonly IIdentityProvider _identityProvider;
        private readonly DataSigningService _dataSigningService;

        /// <summary>
        /// Service manager for creating and managing case graph services.
        /// </summary>
        private readonly InvestigationWorkspaceServiceManager _caseServiceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostedApiController"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics.</param>
        /// <param name="configurationService">Service for case configuration management.</param>
        /// <param name="caseServiceManager">Service manager for case graph operations.</param>
        /// <param name="investigationManagementDbContext">Database context for investigations and queries.</param>
        public AgentAdapterController(
            IConfiguration configuration,
            DataSigningService dataSigningService,
            IIdentityProvider identityProvider,
            InvestigationWorkspaceServiceManager caseServiceManager,
            ILogger<AgentAdapterController> logger)
        {
            _logger = logger;
            _configuration = configuration;
            _identityProvider = identityProvider;
            _caseServiceManager = caseServiceManager;
        }

        [HttpGet]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Open", "invName")]
        public async Task<ActionResult> BeginExecution(string invName)
        {
            if (string.IsNullOrWhiteSpace(invName))
            {
                return BadRequest("Case name is required.");
            }

            string username = GetUserName(User);
            var responseCtx = _caseServiceManager.CreateInvestigationGraphService(invName).CreateContext(username, invName);

            return new JsonResult(responseCtx);
        }

        [HttpPost]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Open", "invName")]
        public async Task<ActionResult> RunExecution(string invName)
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                string data = await reader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(invName))
                {
                    return BadRequest("Case name is required.");
                }

                var invGraphService = _caseServiceManager.CreateInvestigationGraphService(invName);
                AgentInteraction ctxEntry = JsonSerializer.Deserialize<AgentInteraction>(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                if (ctxEntry == null)
                    return null;

                string username = GetUserName(User);

                // Check if the context exists before attempting to add entry
                if (!invGraphService.TryGetContext(ctxEntry.CtxId, username, out _))
                {
                    _logger.LogWarning("Context {CtxId} not found for user {Username}. Client should call BeginExecution to create a new context.", ctxEntry.CtxId, username);
                    return NotFound(new { error = "Context not found. Please reinitialize the context by calling BeginExecution." });
                }

                var ctxFromProvider = invGraphService.AddEntryToContext(ctxEntry.CtxId, username, ctxEntry);

                ctxFromProvider.IsComplete = false;
                var graphSvc = _caseServiceManager.CreateInvestigationGraphService(invName);
                graphSvc.EnqueueAgentRequest(new QueuedAgentRequest(ctxFromProvider, ctxEntry));

                // This will likely change to be a queue to the host processor
                return new JsonResult(ctxEntry);
            }
        }

        private string GetUserName(ClaimsPrincipal user)
        {
            return _identityProvider.GetUserInfoFromClaim(user)?.First(t=>t.IsAuthenticated).UserName ?? throw new ArgumentException("User is not authenticated or does not have a username.");
        }

        [HttpGet]
        [Route("{invName}/{ctxId}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Open", "invName")]
        public async Task<ActionResult> PollExecution(string invName, string ctxId)
        {
            if (string.IsNullOrWhiteSpace(invName))
            {
                return BadRequest("Case name is required.");
            }

            if (!_caseServiceManager.CreateInvestigationGraphService(invName).TryGetContext(ctxId, GetUserName(User), out var responseCtx))
            {
                throw new ArgumentException("Context not found or does not belong to the user.");
            }

            return new JsonResult(responseCtx);
        }
    }
}
