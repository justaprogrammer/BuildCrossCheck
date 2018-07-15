using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Models;
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

        [Theory]
        [MemberData(nameof(ShouldParseLogsCases))]
        public void ShouldParseLogs(string resourceName, StubAnnotation[] expectedAnnotations)
        {
            var resourcePath = GetResourcePath(resourceName);
            File.Exists(resourcePath).ShouldBe(true);

            var parser = new Parser(TestLogger.Create<Parser>(_testOutputHelper));
            var stubAnnotations = parser.Parse(resourcePath);

            stubAnnotations.ShouldBe(expectedAnnotations, false);
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