using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitHubJwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Services;
using MSBLOC.Web.Attributes;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;
using MSBLOC.Web.Services;
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
            services.AddMvc();

            services.Configure<EnvOptions>(Configuration);

            services.AddSingleton<IPrivateKeySource, OptionsPrivateKeySource>();
            services.AddSingleton<Func<string, Task<ICheckRunSubmitter>>>(s => async appOwner =>
            {
                var gitHubAppId = s.GetService<IOptions<EnvOptions>>().Value.GitHubAppId;
                var privateKeySource = s.GetService<IPrivateKeySource>();
                var gitHubTokenGenerator = new TokenGenerator(gitHubAppId, privateKeySource, s.GetService<ILogger<TokenGenerator>>());

                var gitHubClientFactory = new GitHubClientFactory(gitHubTokenGenerator);
                var gitHubClient = await gitHubClientFactory.CreateClientForLogin(appOwner);

                return new CheckRunSubmitter(gitHubClient.Check.Run, s.GetService<ILogger<CheckRunSubmitter>>());
            });

            services.AddScoped<ITempFileService, LocalTempFileService>();
            services.AddScoped<IBinaryLogProcessor, BinaryLogProcessor>();

            services.AddTransient<IMSBLOCService, MSBLOCService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("0.0.1", new Info { Title = "MSBLOC Web API", Version = "0.0.1" });
                c.OperationFilter<MultiPartFormBindingAttribute.MultiPartFormBindingFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSwagger(c => { c.RouteTemplate = "docs/{documentName}/swagger.json"; });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/docs/0.0.1/swagger.json", "MSBLOC Web API");
                c.RoutePrefix = "docs";
            });

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
