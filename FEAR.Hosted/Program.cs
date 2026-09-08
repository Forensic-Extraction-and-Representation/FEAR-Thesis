using FEAR.Domain.Configuration;
using FEAR.Domain.Database;
using FEAR.Host.Core.Agents;
using FEAR.Host.Core.Database;
using FEAR.Host.Core.Identity;
using FEAR.Host.Core.Investigation.Configuration;
using FEAR.Host.Core.ProtectedEncryptionKey;
using FEAR.Host.Core.Services;
using FEAR.Hosted.Domain.Database;
using FEAR.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Validation.AspNetCore;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace FEAR.Hosted
{
    /// <summary>
    /// Entry point for the FEAR.Hosted web application.
    /// Configures web host, services, authentication, authorization, middleware, and database seeding.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for the application. Configures and runs the web host.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var extraConfig = Environment.GetEnvironmentVariable("ASPNETCORE_ADDITIONALCONFIG");
            if (!string.IsNullOrEmpty(extraConfig))
            {
                builder.Configuration.AddJsonFile($"appSettings.{extraConfig}.json", optional: true, reloadOnChange: true);
            }

            var secretsDirectory = Environment.GetEnvironmentVariable("ASPNETCORE_SECRETS_DIRECTORY");
            if (!string.IsNullOrEmpty(secretsDirectory))
            {
                builder.Configuration.AddKeyPerFile(secretsDirectory, optional: true, reloadOnChange: true);
            }

            bool useSsl = false;

            // Configure Kestrel to listen on any IP for both HTTP and HTTPS, with custom ports from configuration.
            builder.WebHost.UseKestrel(options =>
            {
                if (builder.Configuration.GetValue<bool>("ServiceHost:HttpsEnabled", false))
                {
                    options.ListenAnyIP(builder.Configuration.GetValue<int>("https_port", 8009), listenOptions =>
                    {
                        string certificateFile = builder.Configuration.GetValue<string>("ServiceHost:HttpsCertificate", "");
                        string certificateKeyFile = builder.Configuration.GetValue<string>("ServiceHost:HttpsCertificateKey", "");

                        Console.WriteLine($"Using HTTPS certificate: {certificateFile}");
                        Console.WriteLine($"Using HTTPS certificate key: {certificateKeyFile}");

                        if (string.IsNullOrEmpty(certificateKeyFile) || string.IsNullOrEmpty(certificateFile))
                        {
                            listenOptions.UseHttps(); // HTTPS port
                        }
                        else
                        {
                            var x509Certificate2 = new X509Certificate2(certificateFile);
                            var keyContent = File.ReadAllText(certificateKeyFile);
                            var rsa = System.Security.Cryptography.RSA.Create();
                            rsa.ImportFromPem(keyContent.ToCharArray());

                            Console.WriteLine("Using custom HTTPS certificate with private key." + keyContent.Take(50).ToString());
                            Console.WriteLine("Using custom HTTPS certificate with certificate." + x509Certificate2.ToString());

                            x509Certificate2 = x509Certificate2.CopyWithPrivateKey(rsa);
                            listenOptions.UseHttps(x509Certificate2); // Use provided certificate for HTTPS
                        }
                    });
                }
                else
                    options.ListenAnyIP(builder.Configuration.GetSection("ServiceHost:Http").GetValue<int>("ServicePort", 8008)); // HTTP port

                options.Limits.MaxRequestBodySize = Int32.MaxValue; // Increase max request body size from 30 MB
            });

            if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("pgsql"))
                // Add essential services to the DI container.
                builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddAutoMapper(typeof(FEAR.Domain.ActionDefinitions));
            builder.Services.AddSingleton<IdentityDbSeedProvider>();
            builder.Services.AddSingleton<OpenIdDbSeedProvider>();
            builder.Services.AddSingleton<HostedSystemDbSeedProvider>();
            builder.Services.AddSingleton<InvestigationManagementDbSeedProvider>();
            builder.Services.AddSingleton<IPasswordProvider, PasswordProvider>();

            // Configure protected encryption key options from configuration.
            builder.Services.AddProtectedEncryptionKey(options =>
            {
                ProtectedEncryptionKeyBuilderOptions protectedEncryptionKeyBuilderOptions = builder.Configuration.GetSection("Initialization").Get<ProtectedEncryptionKeyBuilderOptions>();

                options.Enabled = protectedEncryptionKeyBuilderOptions.Enabled;
                options.KeySetupEnabled = protectedEncryptionKeyBuilderOptions.KeySetupEnabled;
                options.KeyConfigurationFile = protectedEncryptionKeyBuilderOptions.KeyConfigurationFile;
                options.AutoDecryptSet = protectedEncryptionKeyBuilderOptions.AutoDecryptSet;
            });

            // Configure database contexts for identity and investigation management.
            builder.Services.AddDbContextPool<IdentityDbContext>(options =>
            {
                DatabaseConfigurationOptions dbOptions = builder.Configuration.GetSection("DatabaseConfigurationOptions").Get<DatabaseConfigurationOptions>();
                dbOptions.MigrationAssembly = "FEAR.Hosted";
                var provider = dbOptions.CreateProvider();

                provider.ConstructService(options, dbOptions, builder.Configuration.GetConnectionString("IdentityConnection"));
            });

            builder.Services.AddDbContext<OpenIdApplicationDbContext>(options =>
            {
                options.UseOpenIddict();
                DatabaseConfigurationOptions dbOptions = builder.Configuration.GetSection("DatabaseConfigurationOptions").Get<DatabaseConfigurationOptions>();
                dbOptions.MigrationAssembly = "FEAR.Hosted";
                var provider = dbOptions.CreateProvider();

                provider.ConstructService(options, dbOptions, builder.Configuration.GetConnectionString("OpenIdConnection"));
            });

            builder.Services.AddDbContextPool<InvestigationManagementDbContext>(options =>
                {
                    DatabaseConfigurationOptions dbOptions = builder.Configuration.GetSection("DatabaseConfigurationOptions").Get<DatabaseConfigurationOptions>();
                    dbOptions.MigrationAssembly = "FEAR.Hosted";
                    var provider = dbOptions.CreateProvider();

                provider.ConstructService(options, dbOptions, builder.Configuration.GetConnectionString("InvestigationConnection"));
            });

            builder.Services.AddDbContextPool<HostedSystemDbContext>(options =>
                {
                    DatabaseConfigurationOptions dbOptions = builder.Configuration.GetSection("DatabaseConfigurationOptions").Get<DatabaseConfigurationOptions>();
                    dbOptions.MigrationAssembly = "FEAR.Hosted";
                    var provider = dbOptions.CreateProvider();

                    provider.ConstructService(options, dbOptions, builder.Configuration.GetConnectionString("HostedSystemConnection"));
            });
            // Register scoped services for database and identity/permission providers.
            builder.Services.AddScoped<IInvestigationManagementDbContext>(sp => sp.GetService<InvestigationManagementDbContext>());
            builder.Services.AddScoped<IHostedSystemDbContext>(sp => sp.GetService<HostedSystemDbContext>());
            builder.Services.AddScoped<IIdentityProvider, IdentityProvider>();
            builder.Services.AddScoped<IPermissionProvider, PermissionProvider>();

            // Configure authentication using JWT Bearer tokens and handle HTML requests for login redirection.
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey
                        (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false
                };
                o.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        var isHtmlRequest = context.Request.Headers["Accept"].Any(h => h.Contains("text/html"));
                        if (isHtmlRequest && !context.Response.HasStarted)
                        {
                            context.Response.Redirect($"/Login/Login?returnUrl={context.Request.Path}");
                            context.HandleResponse(); // Stop the default behavior (401 response)
                        }

                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        var isHtmlRequest = context.Request.Headers["Accept"].Any(h => h.Contains("text/html"));
                        if (isHtmlRequest && !context.Response.HasStarted)
                        {
                            context.Response.Redirect($"/Login/Login?returnUrl={context.Request.Path}");
                        }

                        return Task.CompletedTask;
                    }
                };
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/login/login";
                options.LogoutPath = "/login/logout";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
            })
            .AddIdentityCookies(o =>
            {
            });

            builder.Services.AddOpenIddict(opt =>
            {
                opt.AddServer(options =>
                {// Enable the authorization, logout, token and userinfo endpoints.
                    options.SetAuthorizationEndpointUris("connect/authorize")
                           .SetEndSessionEndpointUris("connect/logout")
                           .SetTokenEndpointUris("connect/token")
                           .SetUserInfoEndpointUris("connect/userinfo");

                    // Mark the "email", "profile" and "roles" scopes as supported scopes.
                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                    options.AllowClientCredentialsFlow();

                    options.AllowAuthorizationCodeFlow()
                        .AllowRefreshTokenFlow();

                    options.SetIssuer(builder.Configuration["Jwt:Issuer"]);
                    options.RegisterAudiences(builder.Configuration["Jwt:Audience"]);
                    options.AddSigningKey(new SymmetricSecurityKey
                        (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])));
                    options.AddEncryptionKey(new SymmetricSecurityKey
                        (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:EncKey"])));

                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    var ob = options.UseAspNetCore();

                    if (!builder.Configuration.GetValue<bool>("ServiceHost:HttpsEnabled", false))
                        ob.DisableTransportSecurityRequirement();

                    ob.EnableAuthorizationEndpointPassthrough()
                        .EnableEndSessionEndpointPassthrough()
                        .EnableStatusCodePagesIntegration()
                        .EnableTokenEndpointPassthrough();

                    options.AddEventHandler<OpenIddictServerEvents.ProcessAuthenticationContext>(builder =>
                    {
                        builder.UseInlineHandler(async context =>
                        {
                            if (!string.IsNullOrEmpty(context.Request?.RedirectUri))
                            {
                                var redirectUri = new Uri(context.Request.RedirectUri);
                                if (!context.Request.IsAuthorizationCodeGrantType())
                                    if (context.RequestUri.Authority == redirectUri.Authority)
                                    {
                                        context.SkipRequest();
                                        return;
                                    }
                            }

                            await Task.CompletedTask;
                        });
                    });
                })
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                    .UseDbContext<OpenIdApplicationDbContext>();
                })
                .AddValidation(v =>
                {
                    v.UseLocalServer();
                    v.UseAspNetCore();
                });
            });

            builder.Services.AddRazorPages();
            builder.Services.AddAuthorization();
            builder.Services.AddControllersWithViews();

            builder.Services.AddSingleton<QueryCacheService>();
            builder.Services.AddSingleton<DataSigningService>();
            builder.Services.AddSingleton<InvestigationConfigurationProviderFactory>();
            builder.Services.AddSingleton<InvestigationConfigurationService>();
            builder.Services.AddSingleton<InvestigationWorkspaceServiceManager>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || builder.Environment.IsEnvironment("pgsql"))
            {
                app.UseMigrationsEndPoint();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.AddWebManagementAssets();
            app.UseForwardedHeaders();

            if (app.Configuration.GetValue<bool>("ServiceHost:HttpsEnabled", false))
                app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/administration"), app1 =>
            {
                app1.UseBlazorFrameworkFiles("/administration");
                app1.UseStaticFiles("/administration");

                app1.UseRouting();

                app1.UseEndpoints(endpoints =>
                {
                    endpoints.MapFallbackToFile("/administration/{*path:nonfile}", "administration/index.embedded.html");
                });
            });

            // Middleware to add JWT token from cookie to Authorization header if present.
            app.Use(async (context, next) =>
            {
                var token = context.Request.Cookies["AuthToken"];
                if (!string.IsNullOrWhiteSpace(token) &&
                    !context.Request.Headers.ContainsKey("Authorization"))
                {
                    context.Request.Headers.Append("Authorization", $"Bearer {token}");
                }

                await next();
            });

            app.UseBlazorFrameworkFiles("");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Middleware to add JWT token from cookie to Authorization header if present.
            app.Use(async (context, next) =>
            {
                await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
                await context.AuthenticateAsync(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
                var token = context.Request.Cookies["AuthToken"];
                if (!string.IsNullOrWhiteSpace(token) &&
                    !context.Request.Headers.ContainsKey("Authorization"))
                {
                    context.Request.Headers.Append("Authorization", $"Bearer {token}");
                }

                await next();
            });

            app.MapRazorPages();
            app.MapControllers();
            app.MapDefaultControllerRoute();

            // Seed the databases on startup.
            using (var scope = app.Services.CreateScope())
            {
                var identityDbSeedProvider = scope.ServiceProvider.GetService<IdentityDbSeedProvider>();
                var identityDbContext = scope.ServiceProvider.GetService<IdentityDbContext>();
                identityDbSeedProvider.Seed(identityDbContext, false);

                var openIdDbSeedProvider = scope.ServiceProvider.GetService<OpenIdDbSeedProvider>();
                var openIdDbContext = scope.ServiceProvider.GetService<OpenIdApplicationDbContext>();
                var openIdApplicationManager = scope.ServiceProvider.GetService<IOpenIddictApplicationManager>();
                var openIdAuthorizationManager = scope.ServiceProvider.GetService<IOpenIddictAuthorizationManager>();
                openIdDbSeedProvider.SeedAsync(openIdDbContext, identityDbContext, openIdApplicationManager, openIdAuthorizationManager, builder.Configuration.GetValue<string>("SystemTld"), false).Wait();

                var investigationMgmtDbSeedProvider = scope.ServiceProvider.GetService<InvestigationManagementDbSeedProvider>();
                var investigationMgmtDbContext = scope.ServiceProvider.GetService<InvestigationManagementDbContext>();
                investigationMgmtDbSeedProvider.Seed(investigationMgmtDbContext, identityDbContext, false);

                var hostedSystemDbSeedProvider = scope.ServiceProvider.GetService<HostedSystemDbSeedProvider>();
                var hostedSystemDbContext = scope.ServiceProvider.GetService<HostedSystemDbContext>();
                hostedSystemDbSeedProvider.Seed(hostedSystemDbContext, false);
            }

            app.Run();
        }
    }
}
