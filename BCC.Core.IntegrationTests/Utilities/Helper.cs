namespace BCC.Core.IntegrationTests.Utilities
{
    public static class Helper
    {
        public const string GitHubAppPrivateKeyEnvironmentVariable = "_INTEGRATION_GITHUB_KEY";

        public static string GitHubAppPrivateKey => Environment.GetEnvironmentVariable(GitHubAppPrivateKeyEnvironmentVariable);

        public static string GitHubAppIdString => Environment.GetEnvironmentVariable("_INTEGRATION_GITHUB_APPID");

        public static string IntegrationTestAppOwner => Environment.GetEnvironmentVariable("_INTEGRATION_APP_OWNER");

        public static string IntegrationTestAppRepo => Environment.GetEnvironmentVariable("_INTEGRATION_APP_REPO");

        public static string IntegrationTestAppInstallationId => Environment.GetEnvironmentVariable("_INTEGRATION_APP_INSTALLATION_ID");

        public static string IntegrationTestToken => Environment.GetEnvironmentVariable("_INTEGRATION_TOKEN");

        public static string IntegrationTestUsername => Environment.GetEnvironmentVariable("_INTEGRATION_USERNAME");

        public static int GitHubAppId => Int32.Parse(GitHubAppIdString);

        public static bool HasCredentials =>
            !GitHubAppPrivateKey.IsNullOrEmpty()
            && !GitHubAppIdString.IsNullOrEmpty()
            && !IntegrationTestToken.IsNullOrEmpty()
            && !IntegrationTestUsername.IsNullOrEmpty()
            && !IntegrationTestAppOwner.IsNullOrEmpty()
            && !IntegrationTestAppRepo.IsNullOrEmpty()
            && !IntegrationTestAppInstallationId.IsNullOrEmpty();
    }
}
