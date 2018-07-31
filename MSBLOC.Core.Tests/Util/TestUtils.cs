using System;
using System.IO;
using System.Reflection;
using FluentAssertions;

namespace MSBLOC.Core.Tests.Util
{
    public static class TestUtils
    {
        public static string GetResourcePath(string file)
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            dirPath.Should().NotBeNull();
            return Path.Combine(dirPath, "Resources", file);
        }
    }
}