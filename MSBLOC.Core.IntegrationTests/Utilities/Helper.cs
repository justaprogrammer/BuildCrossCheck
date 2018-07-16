using System;
using Castle.Core.Internal;

namespace MSBLOC.Core.IntegrationTests.Utilities
{
    public static class Helper
    {
        public static string GitHubAppIdString => Environment.GetEnvironmentVariable("MSBLOC_GITHUB_APPID");

        public static int GitHubAppId => Int32.Parse(GitHubAppIdString);

        public static bool HasCredentials => !GitHubAppIdString.IsNullOrEmpty();
    }
}
