using System;
using System.IO;
using System.Linq;
using Bogus;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Model.Builds;
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

        private static readonly Faker Faker;

        public BinaryLogProcessorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<BinaryLogProcessorTests>(testOutputHelper);
        }

        static BinaryLogProcessorTests()
        {
            Faker = new Faker();
        }

        [Fact]
        public void ShouldTestConsoleApp1Warning()
        {
            var cloneRoot = @"C:\projects\testconsoleapp1\";

            var solutionDetails = new SolutionDetails(cloneRoot);

            var project = new ProjectDetails(cloneRoot, @"C:\projects\testconsoleapp1\TestConsoleApp1.sln");
            solutionDetails.Add(project);

            var projectFile = @"C:\projects\testconsoleapp1\TestConsoleApp1\TestConsoleApp1.csproj";
            project = new ProjectDetails(cloneRoot, projectFile);
            project.AddItems("Program.cs", @"Properties\AssemblyInfo.cs");
            solutionDetails.Add(project);

            AssertParseLogs("testconsoleapp1-1warning.binlog",
                solutionDetails, new[]
                {
                    new BuildMessage(BuildMessageLevel.Warning, projectFile, "Program.cs", 13, 13, "The variable 'hello' is assigned but its value is never used", "CS0219"),
                }, cloneRoot);
        }

        [Fact]
        public void ShouldTestConsoleApp1Error()
        {
            var cloneRoot = @"C:\projects\testconsoleapp1\";

            var solutionDetails = new SolutionDetails(cloneRoot);

            var project = new ProjectDetails(cloneRoot, @"C:\projects\testconsoleapp1\TestConsoleApp1.sln");
            solutionDetails.Add(project);

            var projectFile = @"C:\projects\testconsoleapp1\TestConsoleApp1\TestConsoleApp1.csproj";
            project = new ProjectDetails(cloneRoot, projectFile);
            project.AddItems("Program.cs", @"Properties\AssemblyInfo.cs");
            solutionDetails.Add(project);

            AssertParseLogs("testconsoleapp1-1error.binlog",
                solutionDetails, new[]
                {
                    new BuildMessage(BuildMessageLevel.Error, projectFile, "Program.cs", 13, 13, "; expected", "CS1002"),
                }, cloneRoot);
        }

        [Fact]
        public void ShouldMSBLOC()
        {
            var cloneRoot = @"C:\projects\msbuildlogoctokitchecker\";

            var solutionDetails = new SolutionDetails(cloneRoot);

            var project = new ProjectDetails(cloneRoot, @"C:\projects\msbuildlogoctokitchecker\MSBuildLogOctokitChecker.sln");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"C:\projects\msbuildlogoctokitchecker\MSBLOC.Core\MSBLOC.Core.csproj");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"C:\projects\msbuildlogoctokitchecker\MSBLOC.Web\MSBLOC.Web.csproj");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"C:\projects\msbuildlogoctokitchecker\MSBLOC.Web.Tests\MSBLOC.Web.Tests.csproj");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"C:\projects\msbuildlogoctokitchecker\MSBLOC.Core.Tests\MSBLOC.Core.Tests.csproj.metaproj");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"C:\projects\msbuildlogoctokitchecker\MSBLOC.Core.Tests\MSBLOC.Core.Tests.csproj");
            solutionDetails.Add(project);

            AssertParseLogs("msbloc.binlog", solutionDetails, new BuildMessage[0], cloneRoot, options => options.IncludingNestedObjects().IncludingProperties().Excluding(info => info.SelectedMemberInfo.Name == "Paths"));
        }

        [Fact]
        public void ShouldParseOctokitGraphQL()
        {
            var cloneRoot = @"C:\projects\octokit-graphql\";

            var resourcePath = TestUtils.GetResourcePath("octokit.graphql.binlog");
            File.Exists(resourcePath).Should().BeTrue();

            var parser = new BinaryLogProcessor(TestLogger.Create<BinaryLogProcessor>(_testOutputHelper));
            var parsedBinaryLog = parser.ProcessLog(resourcePath, cloneRoot);
        }

        [Fact]
        public void ShouldThrowWhenBuildPathOutisdeCloneRoot()
        {
            var solutionDetails = new SolutionDetails(@"C:\projects\testconsoleapp1\");

            var project = new ProjectDetails(@"C:\projects\testconsoleapp1\", @"C:\projects\testconsoleapp1\TestConsoleApp1.sln");
            solutionDetails.Add(project);

            var projectFile = @"C:\projects\testconsoleapp1\TestConsoleApp1\TestConsoleApp1.csproj";
            project = new ProjectDetails(@"C:\projects\testconsoleapp1\", projectFile);
            project.AddItems("Program.cs", @"Properties\AssemblyInfo.cs");
            solutionDetails.Add(project);

            var projectDetailsException = Assert.Throws<ProjectDetailsException>(() =>
            {
                AssertParseLogs("testconsoleapp1-1warning.binlog",
                    solutionDetails, new BuildMessage[0], @"C:\projects\testconsoleapp2\");
            });

            projectDetailsException.Message.Should().Be(@"Project file path ""C:\projects\testconsoleapp1\TestConsoleApp1.sln"" is not a subpath of ""C:\projects\testconsoleapp2\""");
        }

        private void AssertParseLogs(string resourceName, SolutionDetails expectedSolutionDetails, BuildMessage[] expectedBuildMessages,
            string cloneRoot, Func<EquivalencyAssertionOptions<SolutionDetails>, EquivalencyAssertionOptions<SolutionDetails>> solutionDetailsEquivalency = null)
        {
            var resourcePath = TestUtils.GetResourcePath(resourceName);
            File.Exists(resourcePath).Should().BeTrue();

            var parser = new BinaryLogProcessor(TestLogger.Create<BinaryLogProcessor>(_testOutputHelper));
            var parsedBinaryLog = parser.ProcessLog(resourcePath, cloneRoot);

            parsedBinaryLog.BuildMessages.ToArray().Should().BeEquivalentTo(expectedBuildMessages);

            solutionDetailsEquivalency = solutionDetailsEquivalency ?? (options => options.IncludingNestedObjects().IncludingProperties());
            parsedBinaryLog.SolutionDetails.Should().BeEquivalentTo(expectedSolutionDetails, solutionDetailsEquivalency);
        }
    }
}