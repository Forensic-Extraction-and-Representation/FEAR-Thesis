using FEAR.Host.Core;
using FEAR.Host.Core.Database;
using FEAR.Host.Core.Services;
using FEAR.Domain.Agents.Querying;
using FEAR.Domain.Agents.Attachments;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using System.Text.Json;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace FEAR.Hosted.Controllers
{
    [ApiController]
    [Route("v1.0/[controller]/[action]")]
    public partial class QueryController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Logger for diagnostic and audit purposes.
        /// </summary>
        private readonly ILogger<QueryController> _logger;

        /// <summary>
        /// Service manager for creating and managing case graph services.
        /// </summary>
        private readonly InvestigationWorkspaceServiceManager _caseServiceManager;

        /// <summary>
        /// Service for managing and accessing autopsy case configurations.
        /// </summary>
        private readonly InvestigationConfigurationService _configurationService;

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
        public QueryController(
            IConfiguration configuration,
            ILogger<QueryController> logger,
            QueryCacheService cacheService,
            InvestigationConfigurationService configurationService,
            InvestigationWorkspaceServiceManager caseServiceManager,
            DataSigningService dataSigningService,
            InvestigationManagementDbContext investigationManagementDbContext)
        {
            _logger = logger;
            _configuration = configuration;
            queryCacheService = cacheService;
            _caseServiceManager = caseServiceManager;
            _signingService = dataSigningService;
            _configurationService = configurationService;
            _investigationManagementDbContext = investigationManagementDbContext;
        }
        /// <summary>
        /// Retrieves shared queries for a given case.
        /// Requires authorization for the "Investigation.Open" action on the specified case.
        /// </summary>
        /// <param name="invName">The name of the case.</param>
        /// <returns>A JSON result containing the shared queries for the case.</returns>
        [HttpGet]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Open", "invName")]
        public ActionResult GetSharedQueries(string invName)
        {
            var investigationId = _investigationManagementDbContext.Investigations
                .Where(i => i.Name == invName)
                .Select(i => i.InvestigationId)
                .FirstOrDefault();

            var queries = _investigationManagementDbContext.InvestigationQuerys.Where(t => t.InvestigationId == investigationId);
            return new JsonResult(queries);
        }

        /// <summary>
        /// Handles HTTP OPTIONS requests for CORS preflight, allowing cross-origin requests for query endpoints.
        /// </summary>
        /// <returns>An HTTP 200 OK result with appropriate CORS headers.</returns>
        [HttpOptions]
        public ActionResult QueryOptions()
        {
            // Allow CORS from any origin
            Response.Headers.Add("Access-Control-Allow-Origin", "null");
            //Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, UserID, UserToken");

            return Ok();
        }

        /// <summary>
        /// Executes a SPARQL query against the specified case's graph and returns the results in the appropriate format.
        /// Requires authorization for the "Investigation.Open" action on the specified case.
        /// </summary>
        /// <param name="invName">The name of the case to query.</param>
        /// <returns>The query results as a file in either Turtle or SPARQL JSON format.</returns>
        [HttpPost]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Open", "invName")]
        public async Task<ActionResult> BeginQuery(string invName, [FromBody] GraphQueryRequest graphQueryRequest)
        {
            if (string.IsNullOrWhiteSpace(graphQueryRequest.Query))
            {
                return BadRequest("Query data cannot be empty.");
            }

            GraphQueryAction graphQueryAction = new GraphQueryAction
            {
                Query = graphQueryRequest.Query,
                QueryFormat = graphQueryRequest.QueryFormat
            };

            graphQueryAction.CaseName = invName;
            graphQueryAction.QueryAction = () =>
            {
                var caseGraphService = _caseServiceManager.CreateInvestigationGraphService(invName);

                var queryResult = caseGraphService.Query(graphQueryRequest.Query);
                MemoryStream ms = new MemoryStream();
                TextWriter writer = new StreamWriter(ms, leaveOpen: true);
                string contentType = "";


                if (queryResult is VDS.RDF.Graph)
                {
                    VDS.RDF.Graph g = (VDS.RDF.Graph)queryResult;
                    CompressingTurtleWriter tw = new CompressingTurtleWriter(5, TurtleSyntax.Rdf11Star);
                    tw.DefaultNamespaces.Import(g.NamespaceMap);
                    tw.Save(g, writer);
                    contentType = "text/turtle";
                }
                else
                {
                    SparqlResultSet srs = (SparqlResultSet)queryResult;
                    SparqlJsonWriter sjw = new SparqlJsonWriter();
                    sjw.Save(srs, writer);

                    contentType = "application/sparql-results+json";
                }

                ms.Seek(0, SeekOrigin.Begin);
                return new GraphAttachment
                {
                    Signature = _signingService.GenerateSignature(ms),
                    Format = contentType,
                    Result = new StreamReader(ms).ReadToEnd()
                };
            };

            graphQueryAction.QueryIdentifier = Guid.NewGuid().ToString("N");

            queryCacheService.AddQueryResult(graphQueryAction);

            string responseContent = JsonSerializer.Serialize<GraphQueryRequest>(graphQueryAction, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            return Content(responseContent, "application/json");
        }

        [HttpGet]
        [Route("{invName}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        [InvestigationActionAuthorize("Investigation.Open", "invName")]
        public async Task<ActionResult> EndQuery(string invName, string queryId)
        {
            var request = queryCacheService.GetQueryStream(invName, queryId);
            if (request == null)
            {
                return NotFound("Query not found or has expired.");
            }

            var result = request.QueryAction.Invoke();
            if (result == null || result.Result == null)
            {
                return NotFound("Query execution returned no results.");
            }

            return Ok(result);
        }

    }
}
