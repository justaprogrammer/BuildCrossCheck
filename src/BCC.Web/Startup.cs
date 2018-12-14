using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Claims;
using BCC.Infrastructure.Extensions;
using BCC.Web.Attributes;
using BCC.Web.Authentication;
using BCC.Web.Interfaces;
using BCC.Web.Models;
using BCC.Web.Services;
using BCC.Web.Util;
using Castle.DynamicProxy;
using GitHubJwt;
using Microsoft.ApplicationInsights.AspNetCore.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using CheckRunSubmissionService = BCC.Web.Services.CheckRunSubmissionService;
using GitHubAppClientFactory = BCC.Web.Services.GitHub.GitHubAppClientFactory;
using GitHubAppModelService = BCC.Web.Services.GitHub.GitHubAppModelService;
using GitHubClientFactory = BCC.Web.Services.GitHub.GitHubClientFactory;
using GitHubUserModelService = BCC.Web.Services.GitHub.GitHubUserModelService;
using ICheckRunSubmissionService = BCC.Web.Interfaces.ICheckRunSubmissionService;
using IGitHubAppClientFactory = BCC.Web.Interfaces.GitHub.IGitHubAppClientFactory;
using IGitHubAppModelService = BCC.Web.Interfaces.GitHub.IGitHubAppModelService;
using IGitHubClientFactory = BCC.Web.Interfaces.GitHub.IGitHubClientFactory;
using IGitHubUserClientFactory = BCC.Web.Interfaces.GitHub.IGitHubUserClientFactory;
using IGitHubUserModelService = BCC.Web.Interfaces.GitHub.IGitHubUserModelService;
using ITokenGenerator = BCC.Web.Interfaces.GitHub.ITokenGenerator;
using TokenGenerator = BCC.Web.Services.GitHub.TokenGenerator;

namespace BCC.Web
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ApplicationInsightsLoggerOptions>(Configuration.GetSection("ApplicationInsightsLogger"));
            services.Configure<GitHubAppOptions>(Configuration.GetSection("GitHub:App"));
            services.Configure<AuthOptions>(Configuration.GetSection("Auth"));

            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "PolicyScheme";
                })
                .AddPolicyScheme("PolicyScheme", "Policy Scheme", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            // JWT bearer
                            return AccessTokenAuthenticationHandler.SchemeName;
                        }
                        return CookieAuthenticationDefaults.AuthenticationScheme;
                    };
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/signin";
                    options.LogoutPath = "/signout";
                    options.ForwardChallenge = "GitHub";
                })
                .AddGitHub(options =>
                {
                    options.ClientId = Configuration["GitHub:OAuth:ClientId"];
                    options.ClientSecret = Configuration["GitHub:OAuth:ClientSecret"];
                    options.Scope.Add("user:email");
                    options.Scope.Add("read:org");

                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                    options.ClaimActions.MapJsonKey(CustomClaims.GithubLogin, "login");
                    options.ClaimActions.MapJsonKey(CustomClaims.GithubUrl, "html_url");
                    options.ClaimActions.MapJsonKey(CustomClaims.GithubAvatar, "avatar_url");

                    options.SaveTokens = true;
                })
                .AddScheme<AccessTokenAuthenticationOptions, AccessTokenAuthenticationHandler>(AccessTokenAuthenticationHandler.SchemeName, options =>
                {
                    options.ForwardChallenge = AccessTokenAuthenticationHandler.SchemeName;
                    options.ForwardAuthenticate = AccessTokenAuthenticationHandler.SchemeName;
                });

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.Converters.Add(new OctokitStringEnumConverter());
            });

            services.AddHttpContextAccessor();

            services.AddInfrastructure(Configuration.GetSection("Infrastructure"));

            services.AddSingleton<ITelemetryService, TelemetryService>();

            services.AddSingleton<IPrivateKeySource, GitHubAppOptionsPrivateKeySource>();
            services.AddSingleton<IProxyGenerator, ProxyGenerator>();
            services.AddSingleton<IFileSystem, FileSystem>();
            
            services.AddScoped<ITempFileService, LocalTempFileService>();
            services.AddScoped<ITokenGenerator>(s =>
            {
                var gitHubAppId = s.GetService<IOptions<GitHubAppOptions>>().Value.Id;
                var privateKeySource = s.GetService<IPrivateKeySource>();
                return new TokenGenerator(gitHubAppId, privateKeySource, s.GetService<ILogger<TokenGenerator>>());
            });
            services.AddScoped<IGitHubClientFactory, GitHubClientFactory>();
            services.AddScoped<IGitHubAppClientFactory, GitHubAppClientFactory>();
            services.AddScoped<IGitHubUserClientFactory, GitHubUserClientFactory>();
            services.AddScoped<IGitHubAppModelService, GitHubAppModelService>();
            services.AddScoped<GitHubUserModelService>();
            services.AddScoped<IGitHubUserModelService>(s =>
            {
                var proxyGenerator = s.GetService<IProxyGenerator>();
                var target = s.GetService<GitHubUserModelService>();
                var interceptor = new InMemoryCachingInterceptor();
                return proxyGenerator.CreateInterfaceProxyWithTarget<IGitHubUserModelService>(target, interceptor);
            });
            services.AddScoped<IAccessTokenService, AccessTokenService>();

            services.AddTransient<ICheckRunSubmissionService, CheckRunSubmissionService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("0.0.1", new Info { Title = " Web API", Version = "0.0.1" });
                c.OperationFilter<MultiPartFormBindingAttribute.MultiPartFormBindingFilter>();

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = "header",
                    Name = "Authorization",
                    Type = "apiKey"
                });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var logLevel = Configuration.GetSection("Logging:LogLevel").GetChildren()
                .Where(ll => ll.Key.Equals("Default", StringComparison.InvariantCultureIgnoreCase))
                .Select(ll =>
                {
                    Enum.TryParse(ll.Value, ignoreCase: true, result: out LogLevel logLevelValue);
                    return logLevelValue;
                })
                .FirstOrDefault();

            loggerFactory.AddApplicationInsights(app.ApplicationServices, logLevel);

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHsts();

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "docs/{documentName}/swagger.json";

                //Make routes lower case (Controller names for example)
                c.PreSerializeFilters.Add((document, request) =>
                {
                    document.Paths = document.Paths.ToDictionary(item => item.Key.ToLowerInvariant(), item => item.Value);
                });
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/docs/0.0.1/swagger.json", " Web API");
                c.RoutePrefix = "docs";
            });

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
