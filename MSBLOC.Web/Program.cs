using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

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
                .ConfigureAppConfiguration(new Program().MSBLOCConfigureAppConfiguration)
                .UseStartup<Startup>()
                .Build();

        public void MSBLOCConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder config)
        {
            config.AddEnvironmentVariables("MSBLOC_");

            var builtConfig = config.Build();

            var azureKeyVault = builtConfig["Azure:KeyVault"];

            if (!string.IsNullOrEmpty(azureKeyVault))
            {
                var keyVaultConfigBuilder = GetConfigurationBuilder();

                var keyVaultClient = GetKeyVaultClient();

                keyVaultConfigBuilder.AddAzureKeyVault($"https://{azureKeyVault}.vault.azure.net/", keyVaultClient, new StringUnescapingSecretsManager());
                
                var keyVaultConfig = keyVaultConfigBuilder.Build();

                config.AddConfiguration(keyVaultConfig);
            }
        }

        protected virtual KeyVaultClient GetKeyVaultClient()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            return new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
        }

        protected virtual IConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder();
        }

        private class StringUnescapingSecretsManager : DefaultKeyVaultSecretManager
        {
            public override string GetKey(SecretBundle secret)
            {
                if (secret.ContentType == "text/plain")
                {
                    secret.Value = System.Text.RegularExpressions.Regex.Unescape(secret.Value);
                }
                return base.GetKey(secret);
            }
        }
    }
}
