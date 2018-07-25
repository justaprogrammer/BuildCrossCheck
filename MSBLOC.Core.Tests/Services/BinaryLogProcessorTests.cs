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
    public class BinaryLogProcessorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<BinaryLogProcessorTests> _logger;

        public BinaryLogProcessorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<BinaryLogProcessorTests>(testOutputHelper);
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

        [Fact]
        public void ShouldThrowWhenBuildPathOutisdeCloneRoot()
        {
            var solutionDetails = new SolutionDetails("C:\\projects\\testconsoleapp1\\");

            var project = new ProjectDetails("C:\\projects\\testconsoleapp1\\", @"C:\projects\testconsoleapp1\TestConsoleApp1.sln");
            solutionDetails.Add(project);

            project = new ProjectDetails("C:\\projects\\testconsoleapp1\\", @"C:\projects\testconsoleapp1\TestConsoleApp1\TestConsoleApp1.csproj");
            project.AddItems("Program.cs", @"Properties\AssemblyInfo.cs");
            solutionDetails.Add(project);

            var projectDetailsException = Assert.Throws<ProjectDetailsException>(() =>
            {
                AssertParseLogs("testconsoleapp1-1warning.binlog",
                    solutionDetails, new[]
                    {
                        new Annotation(@"TestConsoleApp1\Program.cs", AnnotationWarningLevel.Warning, "CS0219",
                            "The variable 'hello' is assigned but its value is never used", 13, 13),
                    }, "C:\\projects\\testconsoleapp2\\");
            });

            projectDetailsException.Message.Should().Be(@"Project file path ""C:\projects\testconsoleapp1\TestConsoleApp1.sln"" is not a subpath of ""C:\projects\testconsoleapp2\""");
        }

        private void AssertParseLogs(string resourceName, SolutionDetails expectedSolutionDetails, Annotation[] expectedAnnotations,
            string cloneRoot)
        {
            var resourcePath = TestUtils.GetResourcePath(resourceName);
            File.Exists(resourcePath).Should().BeTrue();

            var parser = new BinaryLogProcessor(TestLogger.Create<BinaryLogProcessor>(_testOutputHelper));
            var parsedBinaryLog = parser.ProcessLog(resourcePath, cloneRoot);

            parsedBinaryLog.Annotations.ToArray().Should().BeEquivalentTo(expectedAnnotations);

            parsedBinaryLog.SolutionDetails.Should().BeEquivalentTo(expectedSolutionDetails, options => options.IncludingNestedObjects().IncludingProperties());
        }
    }
}