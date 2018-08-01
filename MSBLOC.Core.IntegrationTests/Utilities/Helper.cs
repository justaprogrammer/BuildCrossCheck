using System;
using Castle.Core.Internal;

namespace MSBLOC.Core.IntegrationTests.Utilities
{
    public static class Helper
    {
        public const string GitHubAppPrivateKeyEnvironmentVariable = "MSBLOC_GITHUB_KEY";

        public static string GitHubAppPrivateKey => Environment.GetEnvironmentVariable(GitHubAppPrivateKeyEnvironmentVariable);

        public static string GitHubAppIdString => Environment.GetEnvironmentVariable("MSBLOC_GITHUB_APPID");

        public static string IntegrationTestAppOwner => Environment.GetEnvironmentVariable("MSBLOC_INTEGRATION_APP_OWNER");

        public static string IntegrationTestAppName => Environment.GetEnvironmentVariable("MSBLOC_INTEGRATION_APP_NAME");

        public static int GitHubAppId => Int32.Parse(GitHubAppIdString);

        public static bool HasCredentials =>
            !GitHubAppPrivateKey.IsNullOrEmpty()
            && !GitHubAppIdString.IsNullOrEmpty()
            && !IntegrationTestAppOwner.IsNullOrEmpty()
            && !IntegrationTestAppName.IsNullOrEmpty();
    }
}
