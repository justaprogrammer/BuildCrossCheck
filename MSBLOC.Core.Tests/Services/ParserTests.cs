using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
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
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            dirPath.Should().NotBeNull();
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
                }, 
                new Dictionary<string, Dictionary<string, string>>
                {
                    {
                        @"C:\projects\testconsoleapp1\TestConsoleApp1.sln",
                        new Dictionary<string, string>()
                    },
                    {
                        @"C:\projects\testconsoleapp1\TestConsoleApp1\TestConsoleApp1.csproj",
                        new Dictionary<string, string>
                        {
                            {"Program.cs", @"C:\projects\testconsoleapp1\TestConsoleApp1\Program.cs"},
                            {@"Properties\AssemblyInfo.cs", @"C:\projects\testconsoleapp1\TestConsoleApp1\Properties\AssemblyInfo.cs"}
                        }
                    }
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
                new BuildWarningEventArgs[0],
                new Dictionary<string, Dictionary<string, string>>
                {
                    {
                        @"C:\projects\testconsoleapp1\TestConsoleApp1.sln",
                        new Dictionary<string, string>()
                    },
                    {
                        @"C:\projects\testconsoleapp1\TestConsoleApp1\TestConsoleApp1.csproj",
                        new Dictionary<string, string>
                        {
                            {"Program.cs", @"C:\projects\testconsoleapp1\TestConsoleApp1\Program.cs"},
                            {@"Properties\AssemblyInfo.cs", @"C:\projects\testconsoleapp1\TestConsoleApp1\Properties\AssemblyInfo.cs"}
                        }
                    }
                });
        }

        private void AssertParseLogs(string resourceName, BuildErrorEventArgs[] expectedBuildErrorEventArgs, BuildWarningEventArgs[] expectedBuildWarningEventArgs, Dictionary<string, Dictionary<string, string>> expectedProjectFileLookup)
        {
            var resourcePath = GetResourcePath(resourceName);
            File.Exists(resourcePath).Should().BeTrue();

            var parser = new Parser(TestLogger.Create<Parser>(_testOutputHelper));
            var parsedBinaryLog = parser.Parse(resourcePath);

            parsedBinaryLog.Errors.ToArray().Should().BeEquivalentTo(expectedBuildErrorEventArgs, 
                options => options
                    .Excluding(args => args.BuildEventContext)
                    .Excluding(args => args.Timestamp));

            parsedBinaryLog.Warnings.ToArray().Should().BeEquivalentTo(expectedBuildWarningEventArgs, options => 
                options
                    .Excluding(args => args.BuildEventContext)
                    .Excluding(args => args.Timestamp));

            parsedBinaryLog.SolutionDetails.Should().BeEquivalentTo(expectedProjectFileLookup);
        }
    }
}