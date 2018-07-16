using System;
using Castle.Core.Internal;

namespace MSBLOC.Core.IntegrationTests.Utilities
{
    public static class Helper
    {
        public const string GitHubAppPrivateKeyEnvironmentVariable = "MSBLOC_GITHUB_KEY";

        public static string GitHubAppPrivateKey => Environment.GetEnvironmentVariable(GitHubAppPrivateKeyEnvironmentVariable);

        public static string GitHubAppIdString => Environment.GetEnvironmentVariable("MSBLOC_GITHUB_APPID");

        public static int GitHubAppId => Int32.Parse(GitHubAppIdString);

        public static bool HasCredentials => !GitHubAppPrivateKey.IsNullOrEmpty() && !GitHubAppIdString.IsNullOrEmpty();
    }
}
