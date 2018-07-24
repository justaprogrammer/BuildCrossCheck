using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Logging;

namespace MSBLOC.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(MSBLOCConfigureAppConfiguration)
                .UseStartup<Startup>()
                .Build();

        public static void MSBLOCConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder config)
        {
            config.AddEnvironmentVariables("MSBLOC_");

            var builtConfig = config.Build();

            var azureKeyVault = builtConfig["Azure:KeyVault"];

            if (!string.IsNullOrEmpty(azureKeyVault))
            {
                var keyVaultConfigBuilder = new ConfigurationBuilder();

                var azureServiceTokenProvider = new AzureServiceTokenProvider();

                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                keyVaultConfigBuilder.AddAzureKeyVault($"https://{azureKeyVault}.vault.azure.net/",
                    keyVaultClient, new DefaultKeyVaultSecretManager());

                var keyVaultConfig = keyVaultConfigBuilder.Build();

                config.AddConfiguration(keyVaultConfig);
            }
        }
    }
}
