using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Models;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
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

        [TestCaseSource(nameof(ShouldParseLogsCases))]
        public void ShouldParseLogs(string resourceName, StubAnnotation[] expectedAnnotations)
        {
            var resourcePath = GetResourcePath(resourceName);
            FileAssert.Exists(resourcePath);

            var parser = new Parser(TestLogger.Create<Parser>());
            var stubAnnotations = parser.Parse(resourcePath);

            for (var index = 0; index < stubAnnotations.Length; index++)
            {
                var stubAnnotation = stubAnnotations[index];
                var expectedAnnotation = expectedAnnotations[index];
                stubAnnotation.ShouldBe(expectedAnnotation);
            }
        }

        private static IEnumerable<TestCaseData> ShouldParseLogsCases()
        {
            yield return
                CreateTestCase("TestConsoleApp1: 1 Warning", "testconsoleapp1-1warning.binlog",
                    new[]
                    {
                        new StubAnnotation
                        {
                            FileName = "Program.cs",
                            Message = "The variable 'hello' is assigned but its value is never used",
                            WarningLevel = "Warning",
                            Title = "CS0219",
                            StartLine = 13,
                            EndLine = 13
                        }
                    });

            yield return
                CreateTestCase("TestConsoleApp1: 1 Error", "testconsoleapp1-1error.binlog",
                    new[]
                    {
                        new StubAnnotation
                        {
                            FileName = "Program.cs",
                            Message = "; expected",
                            WarningLevel = "Error",
                            Title = "CS1002",
                            StartLine = 13,
                            EndLine = 13
                        }
                    });
        }

        private static TestCaseData CreateTestCase(string testName, string expectedAnnotations,
            StubAnnotation[] stubAnnotations)
        {
            return new TestCaseData(expectedAnnotations, stubAnnotations)
            {
                TestName = testName
            };
        }
    }
}