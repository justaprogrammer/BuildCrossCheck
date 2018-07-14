using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Models;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
using Xunit;

namespace MSBLOC.Core.Tests.Services
{
    public class ParserTests
    {
        private static readonly ILogger<ParserTests> logger = TestLogger.Create<ParserTests>();

        private static string GetResourcePath(string file)
        {
            var location = typeof(ParserTests).GetTypeInfo().Assembly.Location;
            var dirPath = Path.GetDirectoryName(location);
            Assert.NotNull(dirPath);
            return Path.Combine(dirPath, "Resources", file);
        }

        [Theory]
        [MemberData(nameof(ShouldParseLogsCases))]
        public void ShouldParseLogs(string resourceName, StubAnnotation[] expectedAnnotations)
        {
            var resourcePath = GetResourcePath(resourceName);
            Assert.True(File.Exists(resourcePath));

            var parser = new Parser(TestLogger.Create<Parser>());
            var stubAnnotations = parser.Parse(resourcePath);

            for (var index = 0; index < stubAnnotations.Length; index++)
            {
                var stubAnnotation = stubAnnotations[index];
                var expectedAnnotation = expectedAnnotations[index];
                stubAnnotation.ShouldBe(expectedAnnotation);
            }
        }

        public static IEnumerable<object[]> ShouldParseLogsCases
        {
            get
            {
                yield return new object[]
                {
                    "testconsoleapp1-1warning.binlog",
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
                    }
                };

                yield return new object[]
                {
                    "testconsoleapp1-1error.binlog",
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
                    }
                };
            }
        }
    }
}