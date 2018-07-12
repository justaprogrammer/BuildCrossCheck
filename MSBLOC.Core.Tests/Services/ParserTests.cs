using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Services;
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

        [TestCaseSource(nameof(ShouldParseLogsCases))]
        public void ShouldParseLogs(string resourceName, CheckRunAnnotation[] expectedAnnotations)
        {
            var resourcePath = GetResourcePath(resourceName);
            FileAssert.Exists(resourcePath);

            var parser = new Parser(TestLogger.Create<Parser>());
            var checkRunAnnotations = parser.Parse(resourcePath);

            for (var index = 0; index < checkRunAnnotations.Length; index++)
            {
                var checkRunAnnotation = checkRunAnnotations[index];
                var expectedAnnotation = expectedAnnotations[index];
                checkRunAnnotation.ShouldBe(expectedAnnotation);
            }
        }

        private static IEnumerable<TestCaseData> ShouldParseLogsCases()
        {
            yield return
                CreateTestCase("TestConsoleApp1: 1 Warning", "testconsoleapp1-1warning.binlog",
                    new[]
                    {
                        new CheckRunAnnotation("Program.cs", "", 13, 13, CheckWarningLevel.Warning, "The variable 'hello' is assigned but its value is never used")
                        {
                            Title = "CS0219",
                            RawDetails = String.Empty,
                        }
                    });

            yield return
                CreateTestCase("TestConsoleApp1: 1 Error", "testconsoleapp1-1error.binlog",
                    new[]
                    {
                        new CheckRunAnnotation("Program.cs", "", 13, 13, CheckWarningLevel.Failure, "; expected")
                        {
                            Title = "CS1002",
                            RawDetails = String.Empty,
                        }
                    });
        }

        private static TestCaseData CreateTestCase(string testName, string expectedAnnotations,
            CheckRunAnnotation[] stubAnnotations)
        {
            return new TestCaseData(expectedAnnotations, stubAnnotations)
            {
                TestName = testName
            };
        }
    }
}