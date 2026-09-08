using FEAR.Domain.Database;
using FEAR.Domain.Dto.Authentication;
using FEAR.Domain.Dto.ColorMap;
using FEAR.Domain.Dto.InvestigationStore;
using FEAR.Domain.Model;
using FEAR.Host.Core.Identity;
using FEAR.Host.Core.ProtectedEncryptionKey;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace FEAR.Host.Core.Database
{
    /// <summary>
    /// Provides functionality to seed the investigation management database with initial schema or data.
    /// Ensures that the required tables are created for investigation management operations.
    /// </summary>
    public class InvestigationManagementDbSeedProvider
    {
        private InvestigationManagementDbContext _investigationManagementDbContext;
        private IdentityDbContext _identityDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvestigationManagementDbSeedProvider"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration (not used in this implementation).</param>
        /// <param name="passwordProvider">Password provider (not used in this implementation).</param>
        /// <param name="protectedEncryptionKeyProvider">Encryption key provider (not used in this implementation).</param>
        public InvestigationManagementDbSeedProvider(IConfiguration configuration, IPasswordProvider passwordProvider, IProtectedEncryptionKeyProvider protectedEncryptionKeyProvider)
        {
        }

        /// <summary>
        /// Ensures the investigation management database schema is created.
        /// If the database or tables do not exist, they will be created.
        /// </summary>
        /// <param name="investigationManagementDbContext">The database context to seed.</param>
        /// <param name="allowDelete">Indicates if deletion is allowed (not used in this implementation).</param>
        public void Seed(InvestigationManagementDbContext investigationManagementDbContext, IdentityDbContext identityDbContext, bool allowDelete)
        {
            _investigationManagementDbContext = investigationManagementDbContext;
            _identityDbContext = identityDbContext;
            _investigationManagementDbContext.Database.Migrate();

            SeedInvestigationData();
            SeedColorMapData();
        }

        private void SeedInvestigationData()
        {
            try
            {
                int invCount = _investigationManagementDbContext.Investigations.Count();
                string defaultsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "initial_cases.json");
                bool defaultsFileExists = File.Exists(defaultsFilePath);
                User? adminUser = _identityDbContext.Users.FirstOrDefault(t => t.UserName == "admin");

                if (invCount == 0 && defaultsFileExists && adminUser != null)
                {
                    string jsonContent = File.ReadAllText(defaultsFilePath);
                    var defaultCases = System.Text.Json.JsonSerializer.Deserialize<CaseConfiguration[]>(jsonContent);

                    if (defaultCases != null)
                    {
                        foreach (CaseConfiguration dc in defaultCases)
                        {
                            if (dc.GraphManager.RemoteGraph)
                            {
                                // Attempt to extract the base remote graph URL from the configuration
                                string remoteGraphUrl = (dc.Connections.GraphDBConnection["Endpoint"] != null ? ((JsonElement)dc.Connections.GraphDBConnection["Endpoint"]).GetString() : null);
                                string remoteGraphUsername = (dc.Connections.GraphDBConnection["Username"] != null ? ((JsonElement)dc.Connections.GraphDBConnection["Username"]).GetString() : null);
                                string remoteGraphPassword = (dc.Connections.GraphDBConnection["Password"] != null ? ((JsonElement)dc.Connections.GraphDBConnection["Password"]).GetString() : null);

                                // Make a web request to the base remote graph URL to check if it contains the following:
                                var jenaTitleMarker = "<title>Apache Jena Fuseki UI</title>";

                                try
                                {
                                    Uri remoteGraphEndpoint;
                                    bool isJena = false;
                                    if (Uri.TryCreate(remoteGraphUrl, UriKind.Absolute, out remoteGraphEndpoint))
                                    {
                                        using (var httpClient = new HttpClient())
                                        {
                                            var response = httpClient.GetAsync(remoteGraphEndpoint.Scheme + "://" + remoteGraphEndpoint.Authority).Result;
                                            if (response.IsSuccessStatusCode)
                                            {
                                                var content = response.Content.ReadAsStringAsync().Result;
                                                isJena = content.Contains(jenaTitleMarker);
                                            }
                                        }

                                    }
                                    if (isJena)
                                    {
                                        var graphName = remoteGraphEndpoint.Segments[1].TrimEnd('/');
                                        var authorizationHeaderValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{remoteGraphUsername}:{remoteGraphPassword}"));
                                        var authHeader = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authorizationHeaderValue);
                                        var datasetCreationUrl = $"{remoteGraphEndpoint.Scheme}://{remoteGraphEndpoint.Authority}/$/datasets";
                                        StringContent postContent = new StringContent($"dbName={graphName}&dbType=tdb2", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

                                        using (var httpClient = new HttpClient())
                                        {
                                            httpClient.DefaultRequestHeaders.Authorization = authHeader;
                                            var response = httpClient.PostAsync(
                                                datasetCreationUrl,
                                                postContent
                                                ).Result;
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }

                            Guid invId = Guid.NewGuid();
                            string ivName = dc.InvestigationName;

                            var invConfig = new InvestigationConfiguration
                            {
                                InvestigationConfigurationId = Guid.NewGuid(),
                                InvestigationId = invId,
                                HostedUri = String.Empty,
                                Configuration = dc
                            };

                            var investigationInfo = new InvestigationInfo
                            {
                                InvestigationId = invId,
                                Name = ivName,
                                Description = $"Default investigation for {ivName}",
                                CreatedOn = DateTime.UtcNow,
                                LastModifiedOn = DateTime.UtcNow,
                                CreatedByUserId = adminUser.UserId,
                                CaseDate = DateTime.UtcNow,
                                LastModifiedByUserId = adminUser.UserId,
                                Namespace = dc.NamespaceOption,
                                NamespaceAbbrev = dc.NamespaceAbbrevOption
                            };

                            _investigationManagementDbContext.InvestigationConfigurations.Add(invConfig);
                            _investigationManagementDbContext.Investigations.Add(investigationInfo);
                            _investigationManagementDbContext.SaveChanges();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void SeedColorMapData()
        {
            try
            {
                int colorMapSetCount = _investigationManagementDbContext.ColorMaps.Count();
                string colorMapsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "initial_color_maps.json");
                bool colorMapsFileExists = File.Exists(colorMapsFilePath);

                if (colorMapSetCount == 0 && colorMapsFileExists)
                {
                    string jsonContent = File.ReadAllText(colorMapsFilePath);
                    var cms = System.Text.Json.JsonSerializer.Deserialize<ColorMapSet>(jsonContent);
                    if (cms != null)
                    {
                        var colorMapSet = new ColorMapSet
                        {
                            ColorMapSetId = Guid.NewGuid(),
                            Name = cms.Name,
                            Description = cms.Description,
                            EntityDiscriminator = cms.EntityDiscriminator,
                            EntityId = cms.EntityId,
                            ColorMaps = cms.ColorMaps
                        };

                        _investigationManagementDbContext.ColorMaps.Add(colorMapSet);
                        _investigationManagementDbContext.SaveChanges();
                    }
                }
            }
            catch
            {
            }
        }
    }
}
