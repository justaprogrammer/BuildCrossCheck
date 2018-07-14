using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
using NUnit.Framework;
using Octokit;

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
        public void ShouldTestConsoleApp1Warning()
        {
            ShouldParseLogs("testconsoleapp1-1warning.binlog",
                new BuildErrorEventArgs[0],
                new[]
                {
                    new BuildWarningEventArgs(string.Empty, "CS0219", "Program.cs", 13, 20, 0, 0, "The variable 'hello' is assigned but its value is never used", null, "Csc")
                    {
                        ProjectFile = "C:\\projects\\testconsoleapp1\\TestConsoleApp1\\TestConsoleApp1.csproj"
                    },
                });
        }

        [Test]
        public void ShouldTestConsoleApp1Error()
        {
            ShouldParseLogs("testconsoleapp1-1error.binlog",
                new[]
                {
                    new BuildErrorEventArgs(string.Empty, "CS1002", "Program.cs", 13, 34, 0, 0, "; expected", null, "Csc")
                    {
                        ProjectFile = "C:\\projects\\testconsoleapp1\\TestConsoleApp1\\TestConsoleApp1.csproj"
                    }
                },
                new BuildWarningEventArgs[0]);
        }

        private void ShouldParseLogs(string resourceName, BuildErrorEventArgs[] expectedBuildErrorEventArgs, BuildWarningEventArgs[] expectedBuildWarningEventArgs)
        {
            var resourcePath = GetResourcePath(resourceName);
            FileAssert.Exists(resourcePath);

            var parser = new Parser(TestLogger.Create<Parser>());
            var parsedBinaryLog = parser.Parse(resourcePath);

            for (var index = 0; index < parsedBinaryLog.Errors.Count; index++)
            {
                var buildErrorEventArgs = parsedBinaryLog.Errors[index];
                var expectedBuildErrorEventArg = expectedBuildErrorEventArgs[index];
                buildErrorEventArgs.ShouldBe(expectedBuildErrorEventArg);
            }

            for (var index = 0; index < parsedBinaryLog.Warnings.Count; index++)
            {
                var buildWarningEventArgs = parsedBinaryLog.Warnings[index];
                var expectedBuildWarningEventArg = expectedBuildWarningEventArgs[index];
                buildWarningEventArgs.ShouldBe(expectedBuildWarningEventArg);
            }
        }
    }
}