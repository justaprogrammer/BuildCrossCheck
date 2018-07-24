using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Model;
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
            var cloneRoot = "C:\\projects\\testconsoleapp1\\";

            var solutionDetails = new SolutionDetails(cloneRoot);

            var project = new ProjectDetails(cloneRoot, @"C:\projects\testconsoleapp1\TestConsoleApp1.sln");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"C:\projects\testconsoleapp1\TestConsoleApp1\TestConsoleApp1.csproj");
            project.AddItems("Program.cs", @"Properties\AssemblyInfo.cs");
            solutionDetails.Add(project);

            AssertParseLogs("testconsoleapp1-1warning.binlog",
                solutionDetails, new[]
                {
                    new Annotation(@"TestConsoleApp1\Program.cs", AnnotationWarningLevel.Warning, "CS0219", "The variable 'hello' is assigned but its value is never used", 13, 13),
                }, cloneRoot);
        }

        [Fact]
        public void ShouldTestConsoleApp1Error()
        {
            var cloneRoot = "C:\\projects\\testconsoleapp1\\";

            var solutionDetails = new SolutionDetails(cloneRoot);

            var project = new ProjectDetails(cloneRoot, @"C:\projects\testconsoleapp1\TestConsoleApp1.sln");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"C:\projects\testconsoleapp1\TestConsoleApp1\TestConsoleApp1.csproj");
            project.AddItems("Program.cs", @"Properties\AssemblyInfo.cs");
            solutionDetails.Add(project);

            AssertParseLogs("testconsoleapp1-1error.binlog",
                solutionDetails, new[]
                {
                    new Annotation(@"TestConsoleApp1\Program.cs", AnnotationWarningLevel.Failure, "CS1002", "; expected", 13, 13),
                }, cloneRoot);
        }

        private void AssertParseLogs(string resourceName, SolutionDetails expectedSolutionDetails, Annotation[] expectedAnnotations,
            string cloneRoot)
        {
            var resourcePath = GetResourcePath(resourceName);
            File.Exists(resourcePath).Should().BeTrue();

            var parser = new Parser(TestLogger.Create<Parser>(_testOutputHelper));
            var parsedBinaryLog = parser.Parse(resourcePath, cloneRoot);

            parsedBinaryLog.Annotations.ToArray().Should().BeEquivalentTo(expectedAnnotations);

            parsedBinaryLog.SolutionDetails.Should().BeEquivalentTo(expectedSolutionDetails, options => options.IncludingNestedObjects().IncludingProperties());
        }
    }
}