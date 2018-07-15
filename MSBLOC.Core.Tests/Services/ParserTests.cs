using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Core.Tests.Services
{
    public class ParserTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<ParserTests> _logger;

        public ParserTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<ParserTests>(testOutputHelper);
        }

        private static string GetResourcePath(string file)
        {
            var location = typeof(ParserTests).GetTypeInfo().Assembly.Location;
            var dirPath = Path.GetDirectoryName(location);
            dirPath.ShouldNotBeNull();
            return Path.Combine(dirPath, "Resources", file);
        }

        [Fact]
        public void ShouldTestConsoleApp1Warning()
        {
            AssertParseLogs("testconsoleapp1-1warning.binlog",
                new BuildErrorEventArgs[0],
                new[]
                {
                    new BuildWarningEventArgs(string.Empty, "CS0219", "Program.cs", 13, 20, 0, 0, "The variable 'hello' is assigned but its value is never used", null, "Csc")
                    {
                        ProjectFile = "C:\\projects\\testconsoleapp1\\TestConsoleApp1\\TestConsoleApp1.csproj"
                    },
                });
        }

        [Fact]
        public void ShouldTestConsoleApp1Error()
        {
            AssertParseLogs("testconsoleapp1-1error.binlog",
                new[]
                {
                    new BuildErrorEventArgs(string.Empty, "CS1002", "Program.cs", 13, 34, 0, 0, "; expected", null, "Csc")
                    {
                        ProjectFile = "C:\\projects\\testconsoleapp1\\TestConsoleApp1\\TestConsoleApp1.csproj"
                    }
                },
                new BuildWarningEventArgs[0]);
        }

        private void AssertParseLogs(string resourceName, BuildErrorEventArgs[] expectedBuildErrorEventArgs, BuildWarningEventArgs[] expectedBuildWarningEventArgs)
        {
            var resourcePath = GetResourcePath(resourceName);
            File.Exists(resourcePath).ShouldBe(true);

            var parser = new Parser(TestLogger.Create<Parser>(_testOutputHelper));
            var parsedBinaryLog = parser.Parse(resourcePath);

            parsedBinaryLog.Errors.Length.ShouldBe(expectedBuildErrorEventArgs.Length);
            parsedBinaryLog.Warnings.Length.ShouldBe(expectedBuildWarningEventArgs.Length);

            for (var index = 0; index < parsedBinaryLog.Errors.Length; index++)
            {
                var buildErrorEventArgs = parsedBinaryLog.Errors[index];
                var expectedBuildErrorEventArg = expectedBuildErrorEventArgs[index];
                buildErrorEventArgs.ShouldBe(expectedBuildErrorEventArg);
            }

            for (var index = 0; index < parsedBinaryLog.Warnings.Length; index++)
            {
                var buildWarningEventArgs = parsedBinaryLog.Warnings[index];
                var expectedBuildWarningEventArg = expectedBuildWarningEventArgs[index];
                buildWarningEventArgs.ShouldBe(expectedBuildWarningEventArg);
            }

            parsedBinaryLog.Errors.ToArray().ShouldBe(expectedBuildErrorEventArgs, false);
            parsedBinaryLog.Warnings.ToArray().ShouldBe(expectedBuildWarningEventArgs, false);
        }
    }
}