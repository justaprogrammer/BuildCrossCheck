using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Model.CheckRunSubmission;
using MSBLOC.Core.Model.LogAnalyzer;
using MSBLOC.Core.Tests.Util;
using MSBLOC.MSBuildLog.Console.Interfaces;
using MSBLOC.MSBuildLog.Console.Services;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.MSBuildLog.Console.Tests.Services
{
    public class BuildLogProcessorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<BuildLogProcessorTests> _logger;

        private static readonly Faker Faker;

        public BuildLogProcessorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<BuildLogProcessorTests>(testOutputHelper);
        }

        static BuildLogProcessorTests()
        {
            Faker = new Faker();
        }

        [Fact]
        public void ShouldCreateEmptyCheckRun()
        {
            var annotations = new Annotation[0];

            var checkRun = CreateCheckRun(annotations);

            checkRun.Success.Should().BeTrue();
            checkRun.Name.Should().Be("MSBuildLog Analyzer");
            checkRun.Title.Should().Be("MSBuildLog Analysis");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().AllBeEquivalentTo(annotations);
        }

        [Fact]
        public void ShouldCreateCheckRunWithWarning()
        {
            var annotations = new Annotation[]
            {
                new Annotation(
                    Faker.System.FilePath(),
                    CheckWarningLevel.Warning,
                    Faker.Lorem.Word(),
                    Faker.Lorem.Word(),
                    Faker.Random.Int(),
                    Faker.Random.Int())
            };

            var checkRun = CreateCheckRun(annotations);

            checkRun.Success.Should().BeTrue();
            checkRun.Name.Should().Be("MSBuildLog Analyzer");
            checkRun.Title.Should().Be("MSBuildLog Analysis");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        [Fact]
        public void ShouldCreateCheckRunWithFailure()
        {
            var annotations = new Annotation[]
            {
                new Annotation(
                    Faker.System.FilePath(),
                    CheckWarningLevel.Failure,
                    Faker.Lorem.Word(),
                    Faker.Lorem.Word(),
                    Faker.Random.Int(),
                    Faker.Random.Int())
            };

            var checkRun = CreateCheckRun(annotations);

            checkRun.Success.Should().BeFalse();
            checkRun.Name.Should().Be("MSBuildLog Analyzer");
            checkRun.Title.Should().Be("MSBuildLog Analysis");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        [Fact]
        public void ShouldCreateCheckRunWithWarningAndFailure()
        {
            var annotations = new Annotation[]
            {
                new Annotation(
                    Faker.System.FilePath(),
                    CheckWarningLevel.Warning,
                    Faker.Lorem.Word(),
                    Faker.Lorem.Word(),
                    Faker.Random.Int(),
                    Faker.Random.Int()),
                new Annotation(
                    Faker.System.FilePath(),
                    CheckWarningLevel.Failure,
                    Faker.Lorem.Word(),
                    Faker.Lorem.Word(),
                    Faker.Random.Int(),
                    Faker.Random.Int())
            };

            var checkRun = CreateCheckRun(annotations);

            checkRun.Success.Should().BeFalse();
            checkRun.Name.Should().Be("MSBuildLog Analyzer");
            checkRun.Title.Should().Be("MSBuildLog Analysis");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        private CreateCheckRun CreateCheckRun(Annotation[] annotations)
        {
            var inputFile = Faker.System.FilePath();
            var outputFile = Faker.System.FilePath();
            var cloneRoot = Faker.System.DirectoryPath();

            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile(inputFile, new MockFileData(string.Empty, Encoding.UTF8));
            mockFileSystem.AddDirectory(Path.GetDirectoryName(outputFile));


            var binaryLogProcessor = Substitute.For<IBinaryLogProcessor>();
            binaryLogProcessor.ProcessLog(Arg.Any<string>(), Arg.Any<string>())
                .Returns(annotations);

            var buildLogProcessor = new BuildLogProcessor(mockFileSystem, binaryLogProcessor,
                TestLogger.Create<BuildLogProcessor>(_testOutputHelper));

            buildLogProcessor.Proces(inputFile, outputFile, cloneRoot);

            mockFileSystem.FileExists(outputFile).Should().BeTrue();

            var output = mockFileSystem.GetFile(outputFile).TextContents;
            output.Should().NotBeNullOrWhiteSpace();

            var checkRun = JsonConvert.DeserializeObject<CreateCheckRun>(output);
            return checkRun;
        }
    }
}