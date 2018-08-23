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

            var projectFile = @"C:\projects\testconsoleapp1\TestConsoleApp1\TestConsoleApp1.csproj";
            var project = new ProjectDetails(cloneRoot, projectFile);
            project.AddItems("Program.cs", @"Properties\AssemblyInfo.cs");
            solutionDetails.Add(project);

            var parsedBinaryLog = ParseLogs("testconsoleapp1-1warning.binlog", cloneRoot);

            parsedBinaryLog.BuildMessages.ToArray().Should().BeEquivalentTo(new[]
            {
                new BuildMessage(BuildMessageLevel.Warning, projectFile, "Program.cs", 13, 13, "The variable 'hello' is assigned but its value is never used", "CS0219"),
            });

            parsedBinaryLog.SolutionDetails.Should().BeEquivalentTo(solutionDetails,
                options => options.IncludingNestedObjects().IncludingProperties());

            parsedBinaryLog.SolutionDetails.Keys.ToArray().Should().BeEquivalentTo(cloneRoot + @"TestConsoleApp1\TestConsoleApp1.csproj");
        }

        [Fact]
        public void ShouldTestConsoleApp1Error()
        {
            var cloneRoot = @"C:\projects\testconsoleapp1\";

            var solutionDetails = new SolutionDetails(cloneRoot);

            var projectFile = @"C:\projects\testconsoleapp1\TestConsoleApp1\TestConsoleApp1.csproj";
            var project = new ProjectDetails(cloneRoot, projectFile);
            project.AddItems("Program.cs", @"Properties\AssemblyInfo.cs");
            solutionDetails.Add(project);

            var parsedBinaryLog = ParseLogs("testconsoleapp1-1error.binlog", cloneRoot);

            parsedBinaryLog.BuildMessages.ToArray().Should().BeEquivalentTo(new[]
            {
                new BuildMessage(BuildMessageLevel.Error, projectFile, "Program.cs", 13, 13, "; expected", "CS1002"),
            });

            parsedBinaryLog.SolutionDetails.Should().BeEquivalentTo(solutionDetails,
                options => options.IncludingNestedObjects().IncludingProperties());

            parsedBinaryLog.SolutionDetails.Keys.ToArray().Should().BeEquivalentTo(
                cloneRoot + @"TestConsoleApp1\TestConsoleApp1.csproj");
        }

        [Fact]
        public void ShouldMSBLOC()
        {
            var cloneRoot = @"C:\projects\msbuildlogoctokitchecker\";

            var solutionDetails = new SolutionDetails(cloneRoot);

            var project = new ProjectDetails(cloneRoot, @"C:\projects\msbuildlogoctokitchecker\MSBLOC.Core\MSBLOC.Core.csproj");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"C:\projects\msbuildlogoctokitchecker\MSBLOC.Web\MSBLOC.Web.csproj");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"C:\projects\msbuildlogoctokitchecker\MSBLOC.Web.Tests\MSBLOC.Web.Tests.csproj");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"C:\projects\msbuildlogoctokitchecker\MSBLOC.Core.Tests\MSBLOC.Core.Tests.csproj");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"C:\projects\msbuildlogoctokitchecker\MSBLOC.Infrastructure\MSBLOC.Infrastructure.csproj");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"C:\projects\msbuildlogoctokitchecker\MSBLOC.Core.IntegrationTests\MSBLOC.Core.IntegrationTests.csproj");
            solutionDetails.Add(project);

            var parsedBinaryLog = ParseLogs("msbloc.binlog", cloneRoot);

            parsedBinaryLog.SolutionDetails.Should().BeEquivalentTo(solutionDetails,
                options => options.IncludingNestedObjects().IncludingProperties()
                    .Excluding(info => info.SelectedMemberInfo.Name == "Paths"));

            parsedBinaryLog.SolutionDetails.GetProjectItemPath(cloneRoot + @"MSBLOC.Core.Tests\MSBLOC.Core.Tests.csproj", @"Services\GitHubAppModelServiceTests.cs")
                .Should().NotBeNull();

            var list = parsedBinaryLog.BuildMessages
                .GroupBy(message => (message.ProjectFile, message.Code, message.File, message.LineNumber))
                .ToList();

            parsedBinaryLog.BuildMessages.Count.Should().Be(list.Count);
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
        public void ShouldParseDBATools()
        {
            var cloneRoot = @"c:\github\dbatools\bin\projects\dbatools\";

            var resourcePath = TestUtils.GetResourcePath("dbatools.binlog");
            File.Exists(resourcePath).Should().BeTrue();

            var parser = new BinaryLogProcessor(TestLogger.Create<BinaryLogProcessor>(_testOutputHelper));
            var parsedBinaryLog = parser.ProcessLog(resourcePath, cloneRoot, Faker.Lorem.Word(), Faker.Lorem.Word(), Faker.Lorem.Word());

            parsedBinaryLog.SolutionDetails.CloneRoot.Should().Be(cloneRoot);

            var dbaToolsProject = @"c:\github\dbatools\bin\projects\dbatools\dbatools\dbatools.csproj";
            var dbaToolsProjectPath = @"c:\github\dbatools\bin\projects\dbatools\dbatools";

            var dbaToolsTestProject = @"c:\github\dbatools\bin\projects\dbatools\dbatools.Tests\dbatools.Tests.csproj";
            var dbaToolsTestProjectPath = @"c:\github\dbatools\bin\projects\dbatools\dbatools.Tests";

            var dbaToolsProjectDetails = parsedBinaryLog.SolutionDetails[dbaToolsProject];
            dbaToolsProjectDetails.ProjectFile.Should().Be(dbaToolsProject);
            dbaToolsProjectDetails.ProjectDirectory.Should().Be(dbaToolsProjectPath);

            var dbaToolsTestProjectDetails = parsedBinaryLog.SolutionDetails[dbaToolsTestProject];

            dbaToolsTestProjectDetails.ProjectFile.Should().Be(dbaToolsTestProject);
            dbaToolsTestProjectDetails.ProjectDirectory.Should().Be(dbaToolsTestProjectPath);

            parsedBinaryLog.SolutionDetails.Count(project => project.Value.ProjectFile.EndsWith(".csproj")).Should().Be(2);
            parsedBinaryLog.SolutionDetails.Count(project => project.Value.ProjectFile.EndsWith(".sln")).Should().Be(1);
            parsedBinaryLog.Annotations.Should().BeEmpty();
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
                var parsedBinaryLog = ParseLogs("testconsoleapp1-1warning.binlog", @"C:\projects\testconsoleapp2\");

                parsedBinaryLog.BuildMessages.ToArray().Should().BeEquivalentTo(new BuildMessage[0]);

                parsedBinaryLog.SolutionDetails.Should().BeEquivalentTo(solutionDetails, options => options.IncludingNestedObjects().IncludingProperties());
            });

            projectDetailsException.Message.Should().Be(@"Project file path ""C:\projects\testconsoleapp1\TestConsoleApp1\TestConsoleApp1.csproj"" is not a subpath of ""C:\projects\testconsoleapp2\""");
        }

        private BuildDetails ParseLogs(string resourceName, string cloneRoot)
        {
            var resourcePath = TestUtils.GetResourcePath(resourceName);
            File.Exists(resourcePath).Should().BeTrue();

            var parser = new BinaryLogProcessor(TestLogger.Create<BinaryLogProcessor>(_testOutputHelper));
            return parser.ProcessLog(resourcePath, cloneRoot);
        }
    }
}