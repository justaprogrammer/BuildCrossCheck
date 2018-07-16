using System;
using Castle.Core.Internal;

namespace MSBLOC.Core.IntegrationTests.Utilities
{
    public static class Helper
    {
        public static string Username => Environment.GetEnvironmentVariable("MSBLOC_GITHUBUSERNAME");

        public static string OAuthToken => Environment.GetEnvironmentVariable("MSBLOC_OAUTHTOKEN");

        public static bool HasCredentials => true; //!Username.IsNullOrEmpty() && !OAuthToken.IsNullOrEmpty();
    }
}
