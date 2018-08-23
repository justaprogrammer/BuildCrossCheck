using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Services;
using MSBLOC.Core.Services.Factories;
using MSBLOC.Infrastructure.Extensions;
using MSBLOC.Web.Attributes;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;
using MSBLOC.Web.Services;
using MSBLOC.Web.Util;
using Swashbuckle.AspNetCore.Swagger;

namespace MSBLOC.Web
{
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

                    options.ClaimActions.MapAll();
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                    options.ClaimActions.MapJsonKey("urn:github:login", "login");
                    options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
                    options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");

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

            services.AddSingleton<IPrivateKeySource, GitHubAppOptionsPrivateKeySource>();
            services.AddSingleton<IProxyGenerator, ProxyGenerator>();
            
            services.AddScoped<ITempFileService, LocalTempFileService>();
            services.AddScoped<IBinaryLogProcessor, BinaryLogProcessor>();
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

            services.AddTransient<ILogAnalyzer, LogAnalyzerService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("0.0.1", new Info { Title = "MSBLOC Web API", Version = "0.0.1" });
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
                c.SwaggerEndpoint("/docs/0.0.1/swagger.json", "MSBLOC Web API");
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
