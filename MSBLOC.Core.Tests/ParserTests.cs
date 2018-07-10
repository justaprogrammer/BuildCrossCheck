using System.IO;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace MSBLOC.Core.Tests
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
        public void ParseBinaryLogTest()
        {
            var resourcePath = GetResourcePath("testconsoleapp1-warning.binlog");
            FileAssert.Exists(resourcePath);

            var parser = new Parser(TestLogger.Create<Parser>());
            parser.Parse(resourcePath);
        }
    }
}
