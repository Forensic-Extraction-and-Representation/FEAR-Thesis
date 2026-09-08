using AutoMapper;
using FEAR.Api.Controllers;
using FEAR.Domain;
using FEAR.Host.Core;
using FEAR.Host.Core.Database;
using FEAR.Host.Core.Identity;
using FEAR.Host.Core.IdentitySeed;
using FEAR.Host.Core.Services;
using FEAR.Host.Domain.Api.Investigation;
using FEAR.Hosted.Domain.Api.Investigation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using System.Text;

namespace FEAR.Hosted.Controllers
{
    /// <summary>
    /// API controller for managing autopsy case graph operations and investigation queries.
    /// Provides endpoints for opening cases, posting artifacts, retrieving shared queries, handling CORS options, and executing SPARQL queries.
    /// </summary>
    [ApiController]
    [Route("v1.0/[controller]/[action]")]
    public partial class HostedApiController : AuthenticatedControllerBase
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Logger for diagnostic and audit purposes.
        /// </summary>
        private readonly ILogger<HostedApiController> _logger;

        /// <summary>
        /// Automapper instance for mapping between DTOs and domain models.
        /// </summary>
        private readonly IMapper _mapper;

        /// <summary>
        /// Service manager for creating and managing case graph services.
        /// </summary>
        private readonly InvestigationWorkspaceServiceManager _caseServiceManager;

        /// <summary>
        /// Database context for managing investigations and queries.
        /// </summary>
        private readonly InvestigationManagementDbContext _investigationManagementDbContext;

        private readonly QueryCacheService queryCacheService = new QueryCacheService();

        private readonly DataSigningService _signingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostedApiController"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics.</param>
        /// <param name="configurationService">Service for case configuration management.</param>
        /// <param name="caseServiceManager">Service manager for case graph operations.</param>
        /// <param name="investigationManagementDbContext">Database context for investigations and queries.</param>
        public HostedApiController(
            IConfiguration configuration,
            ILogger<HostedApiController> logger,
            QueryCacheService cacheService,
            InvestigationWorkspaceServiceManager caseServiceManager,
            DataSigningService dataSigningService,
            InvestigationManagementDbContext investigationManagementDbContext,
            IMapper mapper,
            IIdentityProvider identityProvider, IPermissionProvider permissionProvider)
            : base(identityProvider, permissionProvider)
        {
            _logger = logger;
            _mapper = mapper;
            _configuration = configuration;
            queryCacheService = cacheService;
            _caseServiceManager = caseServiceManager;
            _signingService = dataSigningService;
            _investigationManagementDbContext = investigationManagementDbContext;
        }

        /// <summary>
        /// Opens a case and generates a case token for the specified case name.
        /// </summary>
        /// <param name="invName">The name of the case to open.</param>
        /// <returns>A JSON result containing the case name and generated token.</returns>
        [HttpGet]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Open", "invName")]
        public IActionResult OpenInvestigation(string invName)
        {
            var caseGraphService = _caseServiceManager.CreateInvestigationGraphService(invName);

            return new JsonResult(new { CaseName = invName });
        }
        
        [HttpGet("{investigationName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult GetInvestigationByName(string investigationName)
        {
            var manageActionResult = CanPerformSystemAction(ActionDefinitions.System.Investigation.List);
            if (!manageActionResult.IsApproved)
                return Unauthorized();
            var investigation = _investigationManagementDbContext.Investigations.FirstOrDefault(i => i.Name == investigationName);
            if (investigation != null)
                return Ok(_mapper.Map<FEAR.Domain.Model.InvestigationStore.InvestigationInfo>(investigation));
            else
                return NotFound();
        }

        /// <summary>
        /// Posts artifacts to the specified case by reading the request body and executing the operation in the case graph service.
        /// </summary>
        /// <param name="invName">The name of the case to post artifacts to.</param>
        /// <returns>An HTTP 200 OK result on success.</returns>
        [HttpPost]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Evidence.Upload", "invName")]
        public async Task<ActionResult> PostArtifacts(string invName)
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                string data = await reader.ReadToEndAsync();

                var caseGraphService = _caseServiceManager.CreateInvestigationGraphService(invName);

                var contentType = Request.ContentType;
                var postArtifactContext = new PostArtifactContext()
                {
                    ContentType = contentType,
                    JsonData = data,
                    HttpContext = Request.HttpContext
                };

                var executionId = caseGraphService.PostArtifacts(postArtifactContext);

                return Ok(new PostArtifactResponse(executionId));
            }
        }

        /// <summary>
        /// Restarts an InvestigationGraphService instance for the specified investigation.
        /// </summary>
        [HttpGet]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Open", "invName")]
        public IActionResult RestartInvestigationGraphService(string invName)
        {
            _caseServiceManager.RestartInvestigationGraphService(invName);
            return Ok(new { Success = true });
        }

        /// <summary>
        /// Stops the InvestigationGraphService instance for the specified investigation.
        /// </summary>
        [HttpGet]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Open", "invName")]
        public IActionResult StopInvestigationGraphService(string invName)
        {
            _caseServiceManager.StopInvestigationGraphService(invName);
            return Ok(new { Success = true });
        }

        /// <summary>
        /// Starts the InvestigationGraphService instance for the specified investigation.
        /// </summary>
        [HttpGet]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Open", "invName")]
        public IActionResult StartInvestigationGraphService(string invName)
        {
            _caseServiceManager.StartInvestigationGraphService(invName);
            return Ok(new { Success = true });
        }

        /// <summary>
        /// Returns the current running state of the InvestigationGraphService for the specified investigation.
        /// </summary>
        [HttpGet]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Open", "invName")]
        public IActionResult GetInvestigationGraphServiceState(string invName)
        {
            var state = _caseServiceManager.GetInvestigationGraphServiceState(invName);
            return Ok(new { State = state.ToString() });
        }


        /// <summary>
        /// Retrieves telemetry signals for an investigation.
        /// Requires the user to have the "Investigation.Update" system action permission.
        /// </summary>
        /// <param name="request">The request containing the investigation ID and query parameters.</param>
        /// <returns>A response containing the telemetry signals, or Unauthorized.</returns>
        [HttpPost]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Update", "invName")]
        public IActionResult QueryInvestigationTelemetry(string invName, [FromBody] QueryInvestigationTelemetry.Request request)
        {
            var auth = CanPerformInvestigationAction(ActionDefinitions.System.Investigation.Update, request.InvestigationId);
            if (!auth.IsApproved) return Unauthorized();

            var inv = _investigationManagementDbContext.Investigations.Find(request.InvestigationId);
            if (inv == null) return NotFound();

            try
            {
                var graphSvc = _caseServiceManager.CreateInvestigationGraphService(inv.Name);
                if (graphSvc == null)
                    return Ok(new QueryInvestigationTelemetry.Response());

                var signals = graphSvc.GetTelemetrySignals(request.Since, request.Count);
                return Ok(new QueryInvestigationTelemetry.Response
                {
                    Signals = signals.Select(s => new QueryInvestigationTelemetry.TelemetrySignalDto
                    {
                        Timestamp = s.Timestamp,
                        SignalType = s.SignalType,
                        SignalSource = s.SignalSource,
                        SignalData = s.SignalData?.ToString() ?? string.Empty
                    }).ToList(),
                    TotalCount = signals.Count
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving telemetry for investigation {InvestigationId}", request.InvestigationId);
                return StatusCode(500, new { Error = "Failed to retrieve telemetry logs" });
            }
        }

        /// <summary>
        /// Returns the current ontology for the specified investigation in Turtle format.
        /// </summary>
        [HttpGet]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Open", "invName")]
        public IActionResult GetInvestigationOntology(string invName)
        {
            try
            {
                var graphSvc = _caseServiceManager.CreateInvestigationGraphService(invName);
                if (graphSvc == null)
                    return Ok(new GetInvestigationOntology.Response { IsAvailable = false });

                var ontology = graphSvc.InvestigationOntology;
                return Ok(new GetInvestigationOntology.Response
                {
                    OntologyContent = ontology,
                    IsAvailable = !string.IsNullOrWhiteSpace(ontology)
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving ontology for investigation {InvestigationName}", invName);
                return StatusCode(500, new { Error = "Failed to retrieve ontology" });
            }
        }

        [HttpGet]
        [Route("{invName}/{executionId:guid}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Evidence.Upload", "invName")]
        public async Task<ActionResult> GetArtifactWorkItemResult(string invName, Guid executionId)
        {
            var caseGraphService = _caseServiceManager.CreateInvestigationGraphService(invName);
            var result = caseGraphService.GetArtifactWorkItemResult(executionId);
            if(result == null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Returns statistics about the materialized knowledge graph for the specified investigation.
        /// </summary>
        [HttpGet]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Open", "invName")]
        public IActionResult GetInvestigationGraphStatistics(string invName)
        {
            try
            {
                var graphSvc = _caseServiceManager.CreateInvestigationGraphService(invName);
                if (graphSvc == null)
                    return Ok(new GetInvestigationGraphStatistics.Response { IsAvailable = false });

                return Ok(graphSvc.GetGraphStatistics());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving graph statistics for investigation {InvestigationName}", invName);
                return StatusCode(500, new { Error = "Failed to retrieve graph statistics" });
            }
        }

        /// <summary>
        /// Returns the full materialized knowledge graph for the specified investigation serialized as Turtle (TTL).
        /// </summary>
        [HttpGet]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Open", "invName")]
        public IActionResult GetInvestigationGraphData(string invName)
        {
            try
            {
                var graphSvc = _caseServiceManager.CreateInvestigationGraphService(invName);
                if (graphSvc == null)
                    return Ok(new GetInvestigationGraphData.Response { IsAvailable = false });

                var turtle = graphSvc.GetGraphAsTurtle();
                return Ok(new GetInvestigationGraphData.Response
                {
                    TurtleContent = turtle,
                    IsAvailable = !string.IsNullOrWhiteSpace(turtle)
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving graph data for investigation {InvestigationName}", invName);
                return StatusCode(500, new { Error = "Failed to retrieve graph data" });
            }
        }
    }
}
