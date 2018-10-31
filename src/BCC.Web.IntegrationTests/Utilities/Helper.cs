using System;

namespace BCC.Web.IntegrationTests.Utilities
{
    public static class Helper
    {
        public const string GitHubAppPrivateKeyEnvironmentVariable = "BCC_INTEGRATION_GITHUB_KEY";

        public static string GitHubAppPrivateKey => Environment.GetEnvironmentVariable(GitHubAppPrivateKeyEnvironmentVariable);

        public static string GitHubAppIdString => Environment.GetEnvironmentVariable("BCC_INTEGRATION_GITHUB_APPID");

        public static string IntegrationTestAppOwner => Environment.GetEnvironmentVariable("BCC_INTEGRATION_APP_OWNER");

        public static string IntegrationTestAppRepo => Environment.GetEnvironmentVariable("BCC_INTEGRATION_APP_REPO");

        public static string IntegrationTestAppInstallationId => Environment.GetEnvironmentVariable("BCC_INTEGRATION_APP_INSTALLATION_ID");

        public static string IntegrationTestToken => Environment.GetEnvironmentVariable("BCC_INTEGRATION_TOKEN");

        public static string IntegrationTestUsername => Environment.GetEnvironmentVariable("BCC_INTEGRATION_USERNAME");

        public static int GitHubAppId => Int32.Parse(GitHubAppIdString);

        public static bool HasCredentials =>
            !string.IsNullOrEmpty(GitHubAppPrivateKey)
            && !string.IsNullOrEmpty(GitHubAppIdString)
            && !string.IsNullOrEmpty(IntegrationTestToken)
            && !string.IsNullOrEmpty(IntegrationTestUsername)
            && !string.IsNullOrEmpty(IntegrationTestAppOwner)
            && !string.IsNullOrEmpty(IntegrationTestAppRepo)
            && !string.IsNullOrEmpty(IntegrationTestAppInstallationId);
    }
}
