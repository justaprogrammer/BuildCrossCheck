using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Interfaces.GitHub;
using MSBLOC.Core.Model;
using MSBLOC.Core.Model.Builds;
using MSBLOC.Core.Model.GitHub;
using MSBLOC.Core.Model.LogAnalyzer;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Core.Tests.Services
{
    public class LogAnalyzerServiceTests
    {
        public LogAnalyzerServiceTests(ITestOutputHelper testOutputHelper)
        {
            _logger = TestLogger.Create<LogAnalyzerServiceTests>(testOutputHelper);
            _testOutputHelper = testOutputHelper;
        }

        static LogAnalyzerServiceTests()
        {
            Faker = new Faker();
        }

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<LogAnalyzerServiceTests> _logger;

        private static readonly Faker Faker;

        private LogAnalyzerService CreateTarget(
            IBinaryLogProcessor binaryLogProcessor = null,
            IGitHubAppModelService gitHubAppModelService = null)
        {
            if (binaryLogProcessor == null) binaryLogProcessor = Substitute.For<IBinaryLogProcessor>();

            if (gitHubAppModelService == null) gitHubAppModelService = Substitute.For<IGitHubAppModelService>();

            return new LogAnalyzerService(binaryLogProcessor, gitHubAppModelService,
                TestLogger.Create<LogAnalyzerService>(_testOutputHelper));
        }

        private static IEnumerable<string> GenerateFileNames()
        {
            while (true) yield return Path.Combine(Faker.Lorem.Word(), Faker.System.FileName("cs"));
        }

        private async Task<IGitHubAppModelService> SubmitBuild(BuildDetails buildDetails, string owner,
            string repository, string sha, LogAnalyzerConfiguration analyzerConfiguration = null)
        {
            var binaryLogProcessor = Substitute.For<IBinaryLogProcessor>();
            binaryLogProcessor.ProcessLog(null, null).ReturnsForAnyArgs(buildDetails);

            var id = Faker.Random.Long();
            var url = Faker.Internet.Url();

            var gitHubAppModelService = Substitute.For<IGitHubAppModelService>();

            gitHubAppModelService.GetLogAnalyzerConfigurationAsync(null, null, null).ReturnsForAnyArgs(analyzerConfiguration);

            gitHubAppModelService.CreateCheckRunAsync(null, null, null, null, null, null, false, null, null, null)
                .ReturnsForAnyArgs(new CheckRun
                {
                    Id = id,
                    Url = url
                });

            var msblocService = CreateTarget(binaryLogProcessor, gitHubAppModelService);

            var expectedCloneRoot = Faker.System.DirectoryPath();
            var expectedBinLogPath = Faker.System.FilePath();

            var checkRun = await msblocService
                .SubmitAsync(owner, repository, sha, expectedCloneRoot, expectedBinLogPath).ConfigureAwait(false);
            checkRun.Id.Should().Be(id);
            checkRun.Url.Should().Be(url);

            binaryLogProcessor.Received(1).ProcessLog(Arg.Is(expectedBinLogPath), Arg.Is(expectedCloneRoot));

            return gitHubAppModelService;
        }

        [Fact]
        public async Task SubmitEmptyBuildDetails()
        {
            var cloneRoot = @"c:" + Faker.System.DirectoryPath().Replace("/", @"\");
            var buildDetails = new BuildDetails(new SolutionDetails(cloneRoot));

            var owner = Faker.Lorem.Word();
            var repository = Faker.Lorem.Word();
            var sha = Faker.Random.String();

            var gitHubAppModelService = await SubmitBuild(buildDetails, owner, repository, sha);

            Received.InOrder(async () =>
            {
                await gitHubAppModelService.Received(1).GetLogAnalyzerConfigurationAsync(Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha));

                await gitHubAppModelService.Received(1).CreateCheckRunAsync(
                    Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha),
                    Arg.Is("MSBuildLog Analyzer"),
                    Arg.Is("MSBuildLog Analysis"),
                    Arg.Is(""),
                    Arg.Is(true),
                    Arg.Any<Annotation[]>(),
                    Arg.Any<DateTimeOffset>(),
                    Arg.Any<DateTimeOffset>());
            });

            var arguments = gitHubAppModelService.ReceivedCalls().Skip(1).First().GetArguments().ToArray();
            var annotations = (Annotation[])arguments[7];
            annotations.Should().BeEquivalentTo(null);
        }

        [Fact]
        public async Task SubmitBuildDetailsWithWarning()
        {
            var cloneRoot = @"c:" + Faker.System.DirectoryPath().Replace("/", @"\");
            var projectPath = Path.Combine(cloneRoot, Faker.Lorem.Word());
            var projectFile = Path.Combine(projectPath, Faker.System.FileName("csproj"));

            var projectDetails = new ProjectDetails(cloneRoot, projectFile);
            var projectCodeFile = Path.Combine(Faker.Lorem.Word(), Faker.System.FileName("cs"));
            projectDetails.AddItems(projectCodeFile);

            var solutionDetails = new SolutionDetails(cloneRoot) { projectDetails };

            var buildDetails = new BuildDetails(solutionDetails);
            var lineNumber = Faker.Random.Int(2);
            var endLineNumber = lineNumber + 1;
            var message = Faker.Lorem.Sentence();
            var messageCode = Faker.Lorem.Word();
            buildDetails.AddMessage(new BuildMessage(BuildMessageLevel.Warning, projectFile, projectCodeFile,
                lineNumber, endLineNumber, message, messageCode));

            var filename = Path.Combine(projectPath, projectCodeFile).Substring(cloneRoot.Length).Replace(@"\", "/").TrimStart('/');
            var owner = Faker.Lorem.Word();
            var repository = Faker.Lorem.Word();
            var sha = Faker.Random.String();

            var gitHubAppModelService = await SubmitBuild(buildDetails, owner, repository, sha);

            Received.InOrder(async () =>
            {
                await gitHubAppModelService.Received(1).GetLogAnalyzerConfigurationAsync(Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha));

                await gitHubAppModelService.Received(1).CreateCheckRunAsync(
                    Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha),
                    Arg.Is("MSBuildLog Analyzer"),
                    Arg.Is("MSBuildLog Analysis"),
                    Arg.Is(""),
                    Arg.Is(true),
                    Arg.Any<Annotation[]>(),
                    Arg.Any<DateTimeOffset>(),
                    Arg.Any<DateTimeOffset>());
            });

            var arguments = gitHubAppModelService.ReceivedCalls().Skip(1).First().GetArguments().ToArray();
            var annotations = (Annotation[])arguments[7];
            annotations.Should().BeEquivalentTo(new Annotation(
                filename,
                CheckWarningLevel.Warning,
                messageCode,
                messageCode + ": " + message,
                lineNumber,
                endLineNumber,
                $"https://github.com/{owner}/{repository}/blob/{sha}/{filename}"));
        }

        [Fact]
        public async Task SubmitBuildDetailsWithWarningAsError()
        {
            var cloneRoot = @"c:" + Faker.System.DirectoryPath().Replace("/", @"\");
            var projectPath = Path.Combine(cloneRoot, Faker.Lorem.Word());
            var projectFile = Path.Combine(projectPath, Faker.System.FileName("csproj"));

            var projectDetails = new ProjectDetails(cloneRoot, projectFile);
            var projectCodeFile = Path.Combine(Faker.Lorem.Word(), Faker.System.FileName("cs"));
            projectDetails.AddItems(projectCodeFile);

            var solutionDetails = new SolutionDetails(cloneRoot) { projectDetails };

            var buildDetails = new BuildDetails(solutionDetails);
            var lineNumber = Faker.Random.Int(2);
            var endLineNumber = lineNumber + 1;
            var message = Faker.Lorem.Sentence();
            var messageCode = Faker.Lorem.Word();
            buildDetails.AddMessage(new BuildMessage(BuildMessageLevel.Warning, projectFile, projectCodeFile,
                lineNumber, endLineNumber, message, messageCode));

            var filename = Path.Combine(projectPath, projectCodeFile).Substring(cloneRoot.Length).Replace(@"\", "/").TrimStart('/');
            var owner = Faker.Lorem.Word();
            var repository = Faker.Lorem.Word();
            var sha = Faker.Random.String();

            var logAnalyzerConfiguration = new LogAnalyzerConfiguration
            {
                Rules = new List<LogAnalyzerRule>
                {
                    new LogAnalyzerRule
                    {
                        Code = messageCode,
                        ReportAs = ReportAs.Error
                    }
                }
            };

            var gitHubAppModelService = await SubmitBuild(buildDetails, owner, repository, sha, logAnalyzerConfiguration);

            Received.InOrder(async () =>
            {
                await gitHubAppModelService.Received(1).GetLogAnalyzerConfigurationAsync(Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha));

                await gitHubAppModelService.Received(1).CreateCheckRunAsync(
                    Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha),
                    Arg.Is("MSBuildLog Analyzer"),
                    Arg.Is("MSBuildLog Analysis"),
                    Arg.Is(""),
                    Arg.Is(false),
                    Arg.Any<Annotation[]>(),
                    Arg.Any<DateTimeOffset>(),
                    Arg.Any<DateTimeOffset>());
            });

            var arguments = gitHubAppModelService.ReceivedCalls().Skip(1).First().GetArguments().ToArray();
            var annotations = (Annotation[])arguments[7];
            annotations.Should().BeEquivalentTo(new Annotation(
                filename,
                CheckWarningLevel.Failure,
                messageCode,
                messageCode + ": " + message,
                lineNumber,
                endLineNumber,
                $"https://github.com/{owner}/{repository}/blob/{sha}/{filename}"));
        }

        [Fact]
        public async Task SubmitBuildDetailsWithWarningIgnored()
        {
            var cloneRoot = @"c:" + Faker.System.DirectoryPath().Replace("/", @"\");
            var projectPath = Path.Combine(cloneRoot, Faker.Lorem.Word());
            var projectFile = Path.Combine(projectPath, Faker.System.FileName("csproj"));

            var projectDetails = new ProjectDetails(cloneRoot, projectFile);
            var projectCodeFile = Path.Combine(Faker.Lorem.Word(), Faker.System.FileName("cs"));
            projectDetails.AddItems(projectCodeFile);

            var solutionDetails = new SolutionDetails(cloneRoot) { projectDetails };

            var buildDetails = new BuildDetails(solutionDetails);
            var lineNumber = Faker.Random.Int(2);
            var endLineNumber = lineNumber + 1;
            var message = Faker.Lorem.Sentence();
            var messageCode = Faker.Lorem.Word();
            buildDetails.AddMessage(new BuildMessage(BuildMessageLevel.Warning, projectFile, projectCodeFile,
                lineNumber, endLineNumber, message, messageCode));

            var filename = Path.Combine(projectPath, projectCodeFile).Substring(cloneRoot.Length).Replace(@"\", "/").TrimStart('/');
            var owner = Faker.Lorem.Word();
            var repository = Faker.Lorem.Word();
            var sha = Faker.Random.String();

            var logAnalyzerConfiguration = new LogAnalyzerConfiguration
            {
                Rules = new List<LogAnalyzerRule>
                {
                    new LogAnalyzerRule
                    {
                        Code = messageCode,
                        ReportAs = ReportAs.Ignore
                    }
                }
            };

            var gitHubAppModelService = await SubmitBuild(buildDetails, owner, repository, sha, logAnalyzerConfiguration);

            Received.InOrder(async () =>
            {
                await gitHubAppModelService.Received(1).GetLogAnalyzerConfigurationAsync(Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha));

                await gitHubAppModelService.Received(1).CreateCheckRunAsync(
                    Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha),
                    Arg.Is("MSBuildLog Analyzer"),
                    Arg.Is("MSBuildLog Analysis"),
                    Arg.Is(""),
                    Arg.Is(true),
                    Arg.Any<Annotation[]>(),
                    Arg.Any<DateTimeOffset>(),
                    Arg.Any<DateTimeOffset>());
            });

            var arguments = gitHubAppModelService.ReceivedCalls().Skip(1).First().GetArguments().ToArray();
            var annotations = (Annotation[])arguments[7];
            annotations.Should().BeNull();
        }

        [Fact]
        public async Task SubmitBuildDetailsWhenCloneRootWithEndingSlash()
        {
            var cloneRoot = @"c:" + Faker.System.DirectoryPath().Replace("/", @"\") + @"\";
            var projectPath = Path.Combine(cloneRoot, Faker.Lorem.Word());
            var projectFile = Path.Combine(projectPath, Faker.System.FileName("csproj"));

            var projectDetails = new ProjectDetails(cloneRoot, projectFile);
            var projectCodeFile = Path.Combine(Faker.Lorem.Word(), Faker.System.FileName("cs"));
            projectDetails.AddItems(projectCodeFile);

            var solutionDetails = new SolutionDetails(cloneRoot) { projectDetails };

            var buildDetails = new BuildDetails(solutionDetails);
            var lineNumber = Faker.Random.Int(2);
            var endLineNumber = lineNumber + 1;
            var message = Faker.Lorem.Sentence();
            var messageCode = Faker.Lorem.Word();
            buildDetails.AddMessage(new BuildMessage(BuildMessageLevel.Warning, projectFile, projectCodeFile,
                lineNumber, endLineNumber, message, messageCode));

            var filename = Path.Combine(projectPath, projectCodeFile).Substring(cloneRoot.Length).Replace(@"\", "/").TrimStart('/');
            var owner = Faker.Lorem.Word();
            var repository = Faker.Lorem.Word();
            var sha = Faker.Random.String();

            var gitHubAppModelService = await SubmitBuild(buildDetails, owner, repository, sha);

            Received.InOrder(async () =>
            {
                await gitHubAppModelService.Received(1).GetLogAnalyzerConfigurationAsync(Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha));

                await gitHubAppModelService.Received(1).CreateCheckRunAsync(
                    Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha),
                    Arg.Is("MSBuildLog Analyzer"),
                    Arg.Is("MSBuildLog Analysis"),
                    Arg.Is(""),
                    Arg.Is(true),
                    Arg.Any<Annotation[]>(),
                    Arg.Any<DateTimeOffset>(),
                    Arg.Any<DateTimeOffset>());
            });

            var arguments = gitHubAppModelService.ReceivedCalls().Skip(1).First().GetArguments().ToArray();
            var annotations = (Annotation[])arguments[7];
            annotations.Should().BeEquivalentTo(new Annotation(
                filename,
                CheckWarningLevel.Warning,
                messageCode,
                messageCode + ": " + message,
                lineNumber,
                endLineNumber,
                $"https://github.com/{owner}/{repository}/blob/{sha}/{filename}"));
        }

        [Fact]
        public async Task SubmitBuildDetailsWithError()
        {
            var cloneRoot = @"c:" + Faker.System.DirectoryPath().Replace("/", @"\");
            var projectPath = Path.Combine(cloneRoot, Faker.Lorem.Word());
            var projectFile = Path.Combine(projectPath, Faker.System.FileName("csproj"));

            var projectDetails = new ProjectDetails(cloneRoot, projectFile);
            var projectCodeFile = Path.Combine(Faker.Lorem.Word(), Faker.System.FileName("cs"));
            projectDetails.AddItems(projectCodeFile);

            var solutionDetails = new SolutionDetails(cloneRoot) { projectDetails };

            var buildDetails = new BuildDetails(solutionDetails);
            var lineNumber = Faker.Random.Int(2);
            var endLineNumber = lineNumber + 1;
            var message = Faker.Lorem.Sentence();
            var messageCode = Faker.Lorem.Word();
            buildDetails.AddMessage(new BuildMessage(BuildMessageLevel.Error, projectFile, projectCodeFile,
                lineNumber, endLineNumber, message, messageCode));

            var filename = Path.Combine(projectPath, projectCodeFile).Substring(cloneRoot.Length).Replace(@"\", "/").TrimStart('/');
            var owner = Faker.Lorem.Word();
            var repository = Faker.Lorem.Word();
            var sha = Faker.Random.String();

            var gitHubAppModelService = await SubmitBuild(buildDetails, owner, repository, sha);

            Received.InOrder(async () =>
            {
                await gitHubAppModelService.Received(1).GetLogAnalyzerConfigurationAsync(Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha));

                await gitHubAppModelService.Received(1).CreateCheckRunAsync(
                    Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha),
                    Arg.Is("MSBuildLog Analyzer"),
                    Arg.Is("MSBuildLog Analysis"),
                    Arg.Is(""),
                    Arg.Is(false),
                    Arg.Any<Annotation[]>(),
                    Arg.Any<DateTimeOffset>(),
                    Arg.Any<DateTimeOffset>());
            });

            var arguments = gitHubAppModelService.ReceivedCalls().Skip(1).First().GetArguments().ToArray();
            var annotations = (Annotation[])arguments[7];
            annotations.Should().BeEquivalentTo(new Annotation(
                filename,
                CheckWarningLevel.Failure,
                messageCode,
                messageCode + ": " + message,
                lineNumber,
                endLineNumber,
                $"https://github.com/{owner}/{repository}/blob/{sha}/{filename}"));
        }

        [Fact]
        public async Task SubmitUnder50BuildDetails()
        {
            var cloneRoot = @"c:" + Faker.System.DirectoryPath().Replace("/", @"\");
            var projectPath = Path.Combine(cloneRoot, Faker.Lorem.Word());
            var projectFile = Path.Combine(projectPath, Faker.System.FileName("csproj"));

            var projectDetails = new ProjectDetails(cloneRoot, projectFile);

            var solutionDetails = new SolutionDetails(cloneRoot) { projectDetails };
            var buildDetails = new BuildDetails(solutionDetails);

            var projectCodeFiles = GenerateFileNames().Distinct().Take(Faker.Random.Int(50, 200)).ToArray();

            projectDetails.AddItems(projectCodeFiles);

            foreach (var projectCodeFile in Faker.PickRandom(projectCodeFiles, Faker.Random.Int(1, 50)))
                buildDetails.AddMessage(new BuildMessage(BuildMessageLevel.Warning, projectFile, projectCodeFile,
                    Faker.Random.Int(2), Faker.Random.Int(2), Faker.Lorem.Sentence(), Faker.Lorem.Word()));

            buildDetails.BuildMessages.Count.Should().BeGreaterThan(0);
            buildDetails.BuildMessages.Count.Should().BeLessOrEqualTo(50);

            var owner = Faker.Lorem.Word();
            var repository = Faker.Lorem.Word();
            var sha = Faker.Random.String();

            var gitHubAppModelService = await SubmitBuild(buildDetails, owner, repository, sha);

            Received.InOrder(async () =>
            {
                await gitHubAppModelService.Received(1).GetLogAnalyzerConfigurationAsync(Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha));

                await gitHubAppModelService.Received(1)
                    .CreateCheckRunAsync(
                        Arg.Is(owner),
                        Arg.Is(repository),
                        Arg.Is(sha),
                        Arg.Is("MSBuildLog Analyzer"),
                        Arg.Is("MSBuildLog Analysis"),
                        Arg.Is(""),
                        Arg.Is(true),
                        Arg.Is<Annotation[]>(annotations => annotations.Length == buildDetails.BuildMessages.Count),
                        Arg.Any<DateTimeOffset>(),
                        Arg.Any<DateTimeOffset>());
            });

            await gitHubAppModelService.DidNotReceive()
                    .UpdateCheckRunAsync(
                        Arg.Any<long>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<Annotation[]>(),
                        Arg.Any<DateTimeOffset?>(),
                        Arg.Any<DateTimeOffset?>());
        }

        [Fact]
        public async Task Submit50To100BuildDetails()
        {
            var cloneRoot = @"c:" + Faker.System.DirectoryPath().Replace("/", @"\");
            var projectPath = Path.Combine(cloneRoot, Faker.Lorem.Word());
            var projectFile = Path.Combine(projectPath, Faker.System.FileName("csproj"));

            var projectDetails = new ProjectDetails(cloneRoot, projectFile);

            var solutionDetails = new SolutionDetails(cloneRoot) { projectDetails };
            var buildDetails = new BuildDetails(solutionDetails);

            var projectCodeFiles = GenerateFileNames().Distinct().Take(Faker.Random.Int(100, 200)).ToArray();

            projectDetails.AddItems(projectCodeFiles);

            foreach (var projectCodeFile in Faker.PickRandom(projectCodeFiles, Faker.Random.Int(51, 100)))
                buildDetails.AddMessage(new BuildMessage(BuildMessageLevel.Warning, projectFile, projectCodeFile,
                    Faker.Random.Int(2), Faker.Random.Int(2), Faker.Lorem.Sentence(), Faker.Lorem.Word()));

            buildDetails.BuildMessages.Count.Should().BeGreaterThan(50);
            buildDetails.BuildMessages.Count.Should().BeLessOrEqualTo(100);

            var owner = Faker.Lorem.Word();
            var repository = Faker.Lorem.Word();
            var sha = Faker.Random.String();

            var gitHubAppModelService = await SubmitBuild(buildDetails, owner, repository, sha);

            Received.InOrder(async () =>
            {
                await gitHubAppModelService.Received(1).GetLogAnalyzerConfigurationAsync(Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha));

                await gitHubAppModelService.Received(1).CreateCheckRunAsync(
                    Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha),
                    Arg.Is("MSBuildLog Analyzer"),
                    Arg.Is("MSBuildLog Analysis"),
                    Arg.Is(""),
                    Arg.Is(true),
                    Arg.Is<Annotation[]>(annotations => annotations.Length == 50),
                    Arg.Any<DateTimeOffset>(),
                    Arg.Any<DateTimeOffset>());

                await gitHubAppModelService.Received(1)
                    .UpdateCheckRunAsync(
                        Arg.Any<long>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Is<Annotation[]>(annotations => annotations.Length == buildDetails.BuildMessages.Count - 50),
                        Arg.Any<DateTimeOffset?>(),
                        Arg.Any<DateTimeOffset?>());
            });
        }

        [Fact]
        public async Task Submit100To150BuildDetails()
        {
            var cloneRoot = @"c:" + Faker.System.DirectoryPath().Replace("/", @"\");
            var projectPath = Path.Combine(cloneRoot, Faker.Lorem.Word());
            var projectFile = Path.Combine(projectPath, Faker.System.FileName("csproj"));

            var projectDetails = new ProjectDetails(cloneRoot, projectFile);

            var solutionDetails = new SolutionDetails(cloneRoot) { projectDetails };
            var buildDetails = new BuildDetails(solutionDetails);

            var projectCodeFiles = GenerateFileNames().Distinct().Take(Faker.Random.Int(200, 300)).ToArray();

            projectDetails.AddItems(projectCodeFiles);

            foreach (var projectCodeFile in Faker.PickRandom(projectCodeFiles, Faker.Random.Int(101, 150)))
                buildDetails.AddMessage(new BuildMessage(BuildMessageLevel.Warning, projectFile, projectCodeFile,
                    Faker.Random.Int(2), Faker.Random.Int(2), Faker.Lorem.Sentence(), Faker.Lorem.Word()));

            buildDetails.BuildMessages.Count.Should().BeGreaterThan(100);
            buildDetails.BuildMessages.Count.Should().BeLessOrEqualTo(150);

            _logger.LogInformation("Build Message Count: {0}", buildDetails.BuildMessages.Count);

            var owner = Faker.Lorem.Word();
            var repository = Faker.Lorem.Word();
            var sha = Faker.Random.String();

            var gitHubAppModelService = await SubmitBuild(buildDetails, owner, repository, sha);

            Received.InOrder(async () =>
            {
                await gitHubAppModelService.Received(1).GetLogAnalyzerConfigurationAsync(Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha));

                await gitHubAppModelService.Received(1).CreateCheckRunAsync(
                    Arg.Is(owner),
                    Arg.Is(repository),
                    Arg.Is(sha),
                    Arg.Is("MSBuildLog Analyzer"),
                    Arg.Is("MSBuildLog Analysis"),
                    Arg.Is(""),
                    Arg.Is(true),
                    Arg.Is<Annotation[]>(annotations => annotations.Length == 50),
                    Arg.Any<DateTimeOffset>(),
                    Arg.Any<DateTimeOffset>());

                await gitHubAppModelService.Received(1)
                    .UpdateCheckRunAsync(
                        Arg.Any<long>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Is<Annotation[]>(annotations => annotations.Length == 50),
                        Arg.Any<DateTimeOffset?>(),
                        Arg.Any<DateTimeOffset?>());

                await gitHubAppModelService.Received(1)
                    .UpdateCheckRunAsync(
                        Arg.Any<long>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Is<Annotation[]>(annotations => annotations.Length == buildDetails.BuildMessages.Count - 100),
                        Arg.Any<DateTimeOffset?>(),
                        Arg.Any<DateTimeOffset?>());
            });
        }
    }
}