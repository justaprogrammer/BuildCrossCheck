using System.IO;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Services;
using NUnit.Framework;

namespace MSBLOC.Core.Tests.Services
{
    [TestFixture]
    public class ParserTests
    {
        private static readonly ILogger<ParserTests> logger = TestLogger.Create<ParserTests>();

        private static string GetResourcePath(string file)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", file);
        }

        [Test]
        public void ShouldParseLog()
        {
            var resourcePath = GetResourcePath("testconsoleapp1-warning.binlog");
            FileAssert.Exists(resourcePath);

            var parser = new Parser(TestLogger.Create<Parser>());
            parser.Parse(resourcePath);
        }
    }
}
