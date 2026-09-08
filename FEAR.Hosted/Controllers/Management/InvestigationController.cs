using AutoMapper;
using FEAR.Api.Controllers;
using FEAR.Blazor.Shared.Components.ConfigurationEditor.Model;
using FEAR.Domain;
using FEAR.Domain.Agents;
using FEAR.Domain.Dto.InvestigationStore;
using FEAR.Domain.Helpers;
using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphDB;
using FEAR.Domain.Model;
using FEAR.Host.Core;
using FEAR.Host.Core.Database;
using FEAR.Host.Core.Identity;
using FEAR.Host.Core.Services;
using FEAR.Host.Domain.Api.Investigation;
using FEAR.Runtime.Agents;
using FEAR.Runtime.KnowledgeGraph.EntitySearch.InMemory;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Nextended.Core.Extensions;
using OpenIddict.Validation.AspNetCore;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FEAR.Hosted.Controllers.Management
{
    /// <summary>
    /// API controller for managing investigations, including creation, update, configuration queries, and listing.
    /// Inherits authentication and permission checks from <see cref="AuthenticatedControllerBase"/>.
    /// </summary>
    [ApiController]
    [Route("v1.0/Management/[controller]/[action]")]
    public class InvestigationController : AuthenticatedControllerBase
    {
        /// <summary>
        /// Logger for diagnostic and audit purposes.
        /// </summary>
        private readonly ILogger<InvestigationController> _logger;

        /// <summary>
        /// Database context for managing investigations and related data.
        /// </summary>
        private readonly InvestigationManagementDbContext _investigationManagementDbContext;

        private readonly InvestigationConfigurationService _investigationConfigurationProvider;

        /// <summary>
        /// Service scope factory for creating scoped service instances.
        /// </summary>
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly IConfiguration _appConfiguration;

        /// <summary>
        /// Automapper instance for mapping between DTOs and domain models.
        /// </summary>
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvestigationManagementController"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics.</param>
        /// <param name="identityProvider">Identity provider for user resolution.</param>
        /// <param name="permissionProvider">Permission provider for permission checks.</param>
        /// <param name="mapper">Automapper for DTO and model mapping.</param>
        /// <param name="investigationDbContext">Database context for investigations.</param>
        /// <param name="serviceScopeFactory">Service scope factory for dependency injection.</param>
        public InvestigationController(
            ILogger<InvestigationController> logger,
            IIdentityProvider identityProvider,
            IPermissionProvider permissionProvider,
            IConfiguration appConfiguration,
            IMapper mapper,
            InvestigationManagementDbContext investigationDbContext,
            InvestigationConfigurationService investigationConfigurationProvider,
            IServiceScopeFactory serviceScopeFactory)
            : base(identityProvider, permissionProvider)
        {
            _logger = logger;
            _mapper = mapper;
            _appConfiguration = appConfiguration;
            _investigationConfigurationProvider = investigationConfigurationProvider;
            _investigationManagementDbContext = investigationDbContext;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Creates a new investigation and its configuration.
        /// Requires the user to have the "Investigation.Create" system action permission.
        /// </summary>
        /// <param name="request">The request containing investigation details and configuration.</param>
        /// <returns>The ID of the created investigation if successful; otherwise, Unauthorized.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult CreateInvestigation([FromBody] CreateInvestigation.Request request)
        {
            var manageActionResult = CanPerformSystemAction(ActionDefinitions.System.Investigation.Create);
            if (manageActionResult.IsApproved)
            {
                var txn = _investigationManagementDbContext.Database.BeginTransaction();
                var investigation = _mapper.Map<InvestigationInfo>(request);
                investigation.InvestigationId = Guid.NewGuid();
                investigation.CreatedByUserId = CurrentUserInfo.Value.First().UserId;
                investigation.CreatedOn = DateTime.UtcNow;
                investigation.LastModifiedByUserId = CurrentUserInfo.Value.First().UserId;
                investigation.LastModifiedOn = DateTime.UtcNow;

                _investigationManagementDbContext.Investigations.Add(investigation);
                _investigationManagementDbContext.SaveChanges();

                var invConf = new InvestigationConfiguration()
                {
                    InvestigationId = investigation.InvestigationId,
                    Configuration = request.Configuration,
                };

                _investigationManagementDbContext.InvestigationConfigurations.Add(invConf);
                _investigationManagementDbContext.SaveChanges();

                txn.Commit();

                return Ok(new CreateInvestigation.Response() { InvestigationId = investigation.InvestigationId });
            }

            return Unauthorized();
        }

        /// <summary>
        /// Updates an existing investigation.
        /// (Implementation placeholder; currently returns OK.)
        /// </summary>
        /// <param name="request">The request containing updated investigation details.</param>
        /// <returns>HTTP 200 OK.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult SaveInvestigation([FromBody] UpdateInvestigation.Request request)
        {
            PerformActionResult manageAction;
            InvestigationInfo dbInv = null;
            if (request.InvestigationId == Guid.Empty)
            {
                manageAction = CanPerformSystemAction(ActionDefinitions.System.Investigation.Create);
                request.CaseDate = DateTime.Now;
            }
            else
            {
                dbInv = _investigationManagementDbContext.Investigations.Find(request.InvestigationId);
                manageAction = CanPerformInvestigationAction(ActionDefinitions.System.Investigation.Update, dbInv.InvestigationId);
            }

            if (!manageAction.IsApproved)
                return Unauthorized();

            if (dbInv == null)
            {
                dbInv = new InvestigationInfo();
                _investigationManagementDbContext.Investigations.Add(dbInv);
            }

            dbInv.CaseDate = request.CaseDate.ToUniversalTime();
            dbInv.CaseNumber = request.CaseNumber;
            dbInv.CaseStatus = request.CaseStatus;
            dbInv.CaseType = request.CaseType;
            dbInv.Description = request.Description;
            dbInv.LastModifiedByUserId = CurrentUserInfo.Value.First().UserId;
            dbInv.LastModifiedOn = DateTime.UtcNow;
            dbInv.Name = request.Name;
            dbInv.Namespace = request.Namespace;
            dbInv.NamespaceAbbrev = request.NamespaceAbbrev;
            dbInv.CreatedByUserId = CurrentUserInfo.Value.First().UserId;
            dbInv.CreatedOn = DateTime.UtcNow;

            _investigationManagementDbContext.SaveChanges();

            _investigationConfigurationProvider.SaveInvestigationConfiguration(request.Name, request.Configuration);

            return Ok(new UpdateInvestigation.Response() { InvestigationId = request.InvestigationId, Success = true });
        }

        [HttpGet("{investigationId:guid}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult DeleteInvestigation(Guid investigationId)
        {
            var deleteAction = CanPerformSystemAction(ActionDefinitions.System.Investigation.Delete);
            if (deleteAction.IsApproved)
            {
                var inv = _investigationManagementDbContext.Investigations.Find(investigationId);
                if (inv != null)
                {
                    _investigationManagementDbContext.Investigations.Remove(inv);
                    var invConf = _investigationManagementDbContext.InvestigationConfigurations.FirstOrDefault(t => t.InvestigationId == investigationId);
                    if (invConf != null)
                        _investigationManagementDbContext.InvestigationConfigurations.Remove(invConf);

                    var queries = _investigationManagementDbContext.InvestigationQuerys.Where(q => q.InvestigationId == investigationId).ToList();
                    if (queries.Any())
                        _investigationManagementDbContext.InvestigationQuerys.RemoveRange(queries);

                    _investigationManagementDbContext.SaveChanges();

                    return Ok(new { Success = true });
                }
                else
                    return NotFound();
            }
            return Unauthorized();
        }

        /// <summary>
        /// Retrieves the configuration for a specific investigation.
        /// Requires the user to have the "Investigation.Update" system action permission.
        /// </summary>
        /// <param name="request">The request containing the investigation ID.</param>
        /// <returns>The configuration if found; otherwise, NotFound or Unauthorized.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult QueryInvestigationConfiguration([FromBody] QueryInvestigationConfiguration.Request request)
        {
            var updateAction = CanPerformInvestigationAction(ActionDefinitions.System.Investigation.Update, request.InvestigationId);
            if (updateAction.IsApproved)
            {
                var invConf = _investigationManagementDbContext.InvestigationConfigurations.FirstOrDefault(t => t.InvestigationId == request.InvestigationId);

                if (invConf != null)
                    return Ok(new QueryInvestigationConfiguration.Response() { Configuration = invConf.Configuration });
                else
                    return NotFound();
            }

            return Unauthorized();
        }

        /// <summary>
        /// Retrieves a list of investigations.
        /// Requires the user to have the "Investigation.List" system action permission.
        /// </summary>
        /// <param name="request">The request containing query parameters.</param>
        /// <returns>A response containing the list of investigations, or Unauthorized.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult QueryInvestigations([FromBody] QueryInvestigations.Request request)
        {
            var manageActionResult = CanPerformSystemAction(ActionDefinitions.System.Investigation.List);
            if (!manageActionResult.IsApproved)
                return Unauthorized();

            var response = new QueryInvestigations.Response();
            response.Results = _investigationManagementDbContext.Investigations.ToList()
                //.Where(i => i.Name.Contains(request.Query))
                .Select(i => _mapper.Map<FEAR.Domain.Model.InvestigationStore.InvestigationInfo>(i)).ToList();

            response.ReturnedResults = response.Results.Count();

            return Ok(response);
        }

        private CaseConfiguration DefaultInvestigationConfiguration()
        {
            string workingName = $"Investigation-{DateTime.Now.ToString("yyyyMMdd")}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            return new CaseConfiguration()
            {
                InvestigationName = "New Investigation",
                PreCompiledDirectoryOption = "PreCompiled",
                ScriptDirectoryOption = "Source",
                PackageDirectoryOption = "Packages",
                WorkingDirectory = workingName,
                NamespaceAbbrevOption = "if",
                NamespaceOption = "http://inv.fear.graph/",
                Repositories = new Dictionary<string, string[]>()
                {
                    { "GFEAR",[] },
                    { "RFEAR",[] }
                },
                PrecompiledLibraries = new Dictionary<string, string[]>
                {
                    { "CFEAR",[] },
                    { "GFEAR",[] }
                },
                GraphManager = new CaseConfiguration.GraphManagerOptions()
                {
                    RemoteGraph = false
                },
                FindEntityStrategyFactory = new CaseConfiguration.FindEntityStrategyFactoryOptions()
                {
                    Local = typeof(DefaultFindEntityStrategyFactory).AssemblyQualifiedName,
                },
                Connections = new CaseConfiguration.ConnectionOptions()
                {
                    GraphDBConnection = new Dictionary<string, object>()
                    {
                        { "Endpoint" , "" },
                        { "Authentication", false },
                        { "Language", "SPARQL" },
                        { "Username", "" },
                        { "Password", "" }
                    }
                },
                AgentConfiguration = new CaseConfiguration.AgentOptions()
                {
                    Agents = new Dictionary<string, AgentDefinition>(),
                }
            };
        }

        [HttpGet("{investigationId:guid}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult GetInvestigationConfiguration(Guid investigationId)
        {
            var updateAction = CanPerformSystemAction(ActionDefinitions.System.Investigation.Update);
            if (updateAction.IsApproved)
            {
                if (investigationId == Guid.Empty)
                    return Ok(DefaultInvestigationConfiguration());

                var investigation = _investigationManagementDbContext.Investigations.Find(investigationId);
                var invConf = _investigationConfigurationProvider.GetInvestigationConfiguration(investigation.Name);
                if (invConf != null)
                    return Ok(invConf);
                else
                    return NotFound();
            }
            return Unauthorized();
        }

        [HttpGet("{investigationId:guid}")]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult GetInvestigationQueries(Guid investigationId)
        {
            var updateAction = CanPerformInvestigationAction(ActionDefinitions.System.Investigation.Update, investigationId);
            if (updateAction.IsApproved)
            {
                var queries = _investigationManagementDbContext.InvestigationQuerys.Where(q => q.InvestigationId == investigationId).ToList();
                return Ok(queries.Select(q => _mapper.Map<InvestigationQuery>(q)).ToList());
            }
            return Unauthorized();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult DeleteInvestigationQuery([FromBody] InvestigationQuery query)
        {
            var deleteAction = CanPerformInvestigationAction(ActionDefinitions.System.Investigation.Update, query.InvestigationId);
            if (deleteAction.IsApproved)
            {
                var dbQuery = _investigationManagementDbContext.InvestigationQuerys.Find(query.InvestigationQueryId);
                if (dbQuery != null)
                {
                    _investigationManagementDbContext.InvestigationQuerys.Remove(dbQuery);
                    _investigationManagementDbContext.SaveChanges();
                    return Ok(new { Success = true });
                }
                else
                    return NotFound();
            }
            return Unauthorized();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult SaveInvestigationQuery([FromBody] InvestigationQuery query)
        {
            var saveAction = CanPerformInvestigationAction(ActionDefinitions.System.Investigation.Update, query.InvestigationId);
            if (saveAction.IsApproved)
            {
                var dbQuery = _investigationManagementDbContext.InvestigationQuerys.Find(query.InvestigationQueryId);
                if (dbQuery == null)
                {
                    dbQuery = new InvestigationQuery();
                    _investigationManagementDbContext.InvestigationQuerys.Add(dbQuery);
                }

                dbQuery.InvestigationId = query.InvestigationId;
                dbQuery.Name = query.Name;
                dbQuery.Description = query.Description;
                dbQuery.QueryText = query.QueryText;
                dbQuery.LastModifiedByUserId = CurrentUserInfo.Value.First().UserId;
                dbQuery.LastModifiedOn = DateTime.UtcNow;
                dbQuery.CreatedByUserId = CurrentUserInfo.Value.First().UserId;
                dbQuery.CreatedOn = DateTime.UtcNow;
                _investigationManagementDbContext.SaveChanges();
                return Ok(dbQuery);
            }
            return Unauthorized();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult ExportCase([FromBody] ExportCase.Request request)
        {
            var manageAction = CanPerformSystemAction(ActionDefinitions.System.Investigation.Update);
            if (!manageAction.IsApproved)
                return Unauthorized();

            var investigation = _investigationManagementDbContext.Investigations.Find(request.InvestigationId);
            if (investigation == null)
                return NotFound();

            var config = _investigationConfigurationProvider.GetInvestigationConfiguration(investigation.Name);
            if (config == null)
                config = DefaultInvestigationConfiguration();

            // Strip excluded sections from a deep-cloned configuration
            var exportConfig = System.Text.Json.JsonSerializer.Deserialize<CaseConfiguration>(
                System.Text.Json.JsonSerializer.Serialize(config))!;

            if (!request.IncludedSections.Contains("Directories"))
            {
                exportConfig.WorkingDirectory = string.Empty;
                exportConfig.PreCompiledDirectoryOption = string.Empty;
                exportConfig.ScriptDirectoryOption = string.Empty;
                exportConfig.PackageDirectoryOption = string.Empty;
                exportConfig.TranspiledSourceOutputRoot = string.Empty;
            }

            if (!request.IncludedSections.Contains("GraphSettings"))
            {
                exportConfig.GraphManager = new CaseConfiguration.GraphManagerOptions();
                exportConfig.FindEntityStrategyFactory = new CaseConfiguration.FindEntityStrategyFactoryOptions();
            }

            if (!request.IncludedSections.Contains("ConnectionSettings"))
                exportConfig.Connections = new CaseConfiguration.ConnectionOptions();

            if (!request.IncludedSections.Contains("Repositories"))
            {
                exportConfig.Repositories = new Dictionary<string, string[]>();
                exportConfig.PrecompiledLibraries = new Dictionary<string, string[]>();
                exportConfig.PackagedSources = new List<string>();
            }

            if (!request.IncludedSections.Contains("AgentConfiguration"))
            {
                exportConfig.AgentConfiguration = new CaseConfiguration.AgentOptions();
            }
            else if (exportConfig.AgentConfiguration != null)
            {
                // Redact any secret that the user chose not to export
                foreach (var secretEntry in request.AgentSecrets.Where(s => !s.IsIncluded))
                {
                    if (exportConfig.AgentConfiguration.Agents.TryGetValue(secretEntry.AgentKey, out var agentDef)
                        && agentDef.AgentOptions.ContainsKey(secretEntry.FieldName))
                    {
                        agentDef.AgentOptions[secretEntry.FieldName] = string.Empty;
                    }
                }
            }

            var package = new CaseExportPackage
            {
                Manifest = new CaseExportManifest
                {
                    ImportMessage = request.ImportMessage,
                    IncludedSections = request.IncludedSections,
                    AgentSecrets = request.AgentSecrets
                },
                CaseInfo = new CaseExportInfo
                {
                    Name = investigation.Name,
                    Description = investigation.Description,
                    CaseNumber = investigation.CaseNumber,
                    CaseType = investigation.CaseType,
                    CaseStatus = investigation.CaseStatus,
                    CaseDate = investigation.CaseDate,
                    Namespace = investigation.Namespace,
                    NamespaceAbbrev = investigation.NamespaceAbbrev
                },
                Configuration = exportConfig
            };

            if (request.IncludedSections.Contains("Queries"))
            {
                package.Queries = _investigationManagementDbContext.InvestigationQuerys
                    .Where(q => q.InvestigationId == request.InvestigationId)
                    .Select(q => new CaseExportQuery { Name = q.Name, Description = q.Description, QueryText = q.QueryText })
                    .ToList();
            }

            return Ok(new ExportCase.Response { Package = package });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult ImportCase([FromBody] ImportCase.Request request)
        {
            var manageAction = CanPerformSystemAction(ActionDefinitions.System.Investigation.Create);
            if (!manageAction.IsApproved)
                return Unauthorized();

            var package = request.Package;
            if (package?.CaseInfo == null)
                return BadRequest("Invalid export package.");

            // Apply user-supplied secrets for fields that were not exported
            if (package.Configuration?.AgentConfiguration != null)
            {
                foreach (var secret in package.Manifest.AgentSecrets.Where(s => !s.IsIncluded))
                {
                    var secretKey = $"{secret.AgentKey}:{secret.FieldName}";
                    if (request.SuppliedSecrets.TryGetValue(secretKey, out var secretValue)
                        && package.Configuration.AgentConfiguration.Agents.TryGetValue(secret.AgentKey, out var agentDef))
                    {
                        agentDef.AgentOptions[secret.FieldName] = secretValue;
                    }
                }
            }

            // Resolve a unique case name: if the desired name is taken, use "Copy X <name>"
            var desiredName = package.CaseInfo.Name;
            var existingNames = _investigationManagementDbContext.Investigations
                .Select(i => i.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var caseName = desiredName;
            if (existingNames.Contains(caseName))
            {
                var copyNumber = 1;
                while (existingNames.Contains($"Copy {copyNumber} {desiredName}"))
                    copyNumber++;
                caseName = $"Copy {copyNumber} {desiredName}";
            }

            // Step 1: Persist the investigation record.
            var investigation = new FEAR.Domain.Dto.InvestigationStore.InvestigationInfo
            {
                InvestigationId = Guid.NewGuid(),
                Name = caseName,
                Description = package.CaseInfo.Description,
                CaseNumber = package.CaseInfo.CaseNumber,
                CaseType = package.CaseInfo.CaseType,
                CaseStatus = package.CaseInfo.CaseStatus,
                CaseDate = package.CaseInfo.CaseDate.ToUniversalTime(),
                Namespace = package.CaseInfo.Namespace,
                NamespaceAbbrev = package.CaseInfo.NamespaceAbbrev,
                CreatedByUserId = CurrentUserInfo.Value.First().UserId,
                CreatedOn = DateTime.UtcNow,
                LastModifiedByUserId = CurrentUserInfo.Value.First().UserId,
                LastModifiedOn = DateTime.UtcNow
            };

            try
            {
                _investigationManagementDbContext.Investigations.Add(investigation);

                foreach (var q in package.Queries ?? [])
                {
                    _investigationManagementDbContext.InvestigationQuerys.Add(new FEAR.Domain.Dto.InvestigationStore.InvestigationQuery
                    {
                        InvestigationQueryId = Guid.NewGuid(),
                        InvestigationId = investigation.InvestigationId,
                        Name = q.Name,
                        Description = q.Description,
                        QueryText = q.QueryText,
                        CreatedByUserId = CurrentUserInfo.Value.First().UserId,
                        CreatedOn = DateTime.UtcNow,
                        LastModifiedByUserId = CurrentUserInfo.Value.First().UserId,
                        LastModifiedOn = DateTime.UtcNow
                    });
                }

                _investigationManagementDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ImportCase.Response { Success = false, Message = $"Failed to create investigation: {ex.Message}" });
            }

            // Step 2: Save the configuration via the provider.
            // If this fails, manually remove the investigation and queries that were just committed.
            var importedConfig = package.Configuration ?? DefaultInvestigationConfiguration();
            importedConfig.InvestigationName = investigation.Name;

            try
            {
                _investigationConfigurationProvider.SaveInvestigationConfiguration(investigation.Name, importedConfig);
            }
            catch (Exception ex)
            {
                try
                {
                    var queriesToDelete = _investigationManagementDbContext.InvestigationQuerys
                        .Where(q => q.InvestigationId == investigation.InvestigationId).ToList();
                    if (queriesToDelete.Any())
                        _investigationManagementDbContext.InvestigationQuerys.RemoveRange(queriesToDelete);
                    _investigationManagementDbContext.Investigations.Remove(investigation);
                    _investigationManagementDbContext.SaveChanges();
                }
                catch { /* best-effort rollback */ }

                return StatusCode(500, new ImportCase.Response { Success = false, Message = $"Failed to save configuration: {ex.Message}" });
            }

            return Ok(new ImportCase.Response
            {
                Success = true,
                InvestigationId = investigation.InvestigationId,
                Message = $"Investigation '{investigation.Name}' imported successfully."
            });
        }

        [HttpGet]
        public IActionResult GetConfigurationFields()
        {
            var options = new List<ConfigurationField>()
            {
                new ConfigurationField { Name = "GraphSetupProviders", FieldOptions = TypeHelpers.GetAllDerivedTypes<IGraphSetupProvider>().Select(t => new ConfigurationFieldOption { Name = t.FullName, Value = t.AssemblyQualifiedName }).ToList() },
                new ConfigurationField { Name = "LocalFindEntityStrategyFactories", FieldOptions = TypeHelpers.GetAllDerivedTypes<ILocalGraphFindEntityStrategyFactory>().Select(t => new ConfigurationFieldOption { Name = t.FullName, Value = t.AssemblyQualifiedName }).ToList() },
                new ConfigurationField { Name = "RemoteFindEntityStrategyFactories", FieldOptions = TypeHelpers.GetAllDerivedTypes<IRemoteGraphFindEntityStrategyFactory>().Select(t => new ConfigurationFieldOption { Name = t.FullName, Value = t.AssemblyQualifiedName }).ToList() },
                new ConfigurationField { Name = "AgentAdapterTypes", FieldOptions = TypeHelpers.GetAllDerivedTypes<IAgentAdapter>().Select(t => {
                    var agentAttribute = t.GetCustomAttribute<RegisteredAgentAdapterAttribute>();
                    var agentOptionAttributes = t.GetCustomAttributes<RegisteredAgentAdapterFieldAttribute>();
                    var name = agentAttribute?.Name;

                    if(string.IsNullOrEmpty(name))
                        return new ConfigurationFieldOption {Name = null, Value = null };

                    return new ConfigurationFieldOption { Name = name, Value = t.AssemblyQualifiedName,
                        FieldMetadata = agentOptionAttributes.Select(a =>
                        new FieldMetadata {
                            Name = a.FieldName,
                            IsRequired = a.IsRequired,
                            IsSecret = a.IsSecret,
                            Description = a.Description,
                            Values = t.GetCustomAttributes<RegisteredAgentAdapterFieldOptionAttribute>().Where(o => o.FieldName == a.FieldName)
                                .Select(o => new MetadataValue { Value = o.SelectValue, Name = o.DisplayValue, Description = o.Description }).ToArray()
                        }).ToArray()
                    };
                }).Where(t=>t.Name != null).ToList()
                }
            };
            return Ok(options);
        }
    }
}
