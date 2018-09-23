using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using BCC.Core.Model.CheckRunSubmission;
using BCC.Core.Tests.Util;
using BCC.MSBuildLog.Console.Interfaces;
using BCC.MSBuildLog.Console.Services;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace BCC.MSBuildLog.Console.Tests.Services
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

            var checkRun = GetCheckRun(CreateMockBinaryLogProcessor(annotations));

            checkRun.Success.Should().BeTrue();
            checkRun.Name.Should().Be("MSBuildLog Analyzer");
            checkRun.Title.Should().Be("MSBuildLog Analysis");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().AllBeEquivalentTo(annotations);
        }

        [Fact]
        public void ShouldCreateCheckRunWithWarning()
        {
            var annotations = new[]
            {
                new Annotation(
                    Faker.System.FilePath(),
                    CheckWarningLevel.Warning,
                    Faker.Lorem.Word(), Faker.Lorem.Word(),
                    Faker.Random.Int(), Faker.Random.Int())
            };

            var checkRun = GetCheckRun(CreateMockBinaryLogProcessor(annotations));

            checkRun.Success.Should().BeTrue();
            checkRun.Name.Should().Be("MSBuildLog Analyzer");
            checkRun.Title.Should().Be("MSBuildLog Analysis");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        [Fact]
        public void ShouldCreateCheckRunWithFailure()
        {
            var annotations = new[]
            {
                new Annotation(
                    Faker.System.FilePath(),
                    CheckWarningLevel.Failure,
                    Faker.Lorem.Word(), Faker.Lorem.Word(),
                    Faker.Random.Int(), Faker.Random.Int())
            };

            var checkRun = GetCheckRun(CreateMockBinaryLogProcessor(annotations));

            checkRun.Success.Should().BeFalse();
            checkRun.Name.Should().Be("MSBuildLog Analyzer");
            checkRun.Title.Should().Be("MSBuildLog Analysis");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        [Fact]
        public void ShouldCreateCheckRunWithWarningAndFailure()
        {
            var annotations = new[]
            {
                    new Annotation(
                        Faker.System.FilePath(),
                        CheckWarningLevel.Warning,
                        Faker.Lorem.Word(), Faker.Lorem.Word(),
                        Faker.Random.Int(), Faker.Random.Int()),
                    new Annotation(
                        Faker.System.FilePath(),
                        CheckWarningLevel.Failure,
                        Faker.Lorem.Word(), Faker.Lorem.Word(),
                        Faker.Random.Int(), Faker.Random.Int())
            };

            var checkRun = GetCheckRun(CreateMockBinaryLogProcessor(annotations));

            checkRun.Success.Should().BeFalse();
            checkRun.Name.Should().Be("MSBuildLog Analyzer");
            checkRun.Title.Should().Be("MSBuildLog Analysis");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        [Fact]
        public void ShouldCreateCheckRunTestConsoleApp1Warning()
        {
            var annotations = new[]
            {
                new Annotation(
                    "TestConsoleApp1\\Program.cs",
                    CheckWarningLevel.Warning, "CS0219",
                    "The variable 'hello' is assigned but its value is never used",
                    13, 13)
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var resourcePath = TestUtils.GetResourcePath("testconsoleapp1-1warning.binlog");

            var checkRun = GetCheckRun(new BinaryLogProcessor(TestLogger.Create<BinaryLogProcessor>(_testOutputHelper)), resourcePath, cloneRoot);

            checkRun.Success.Should().BeTrue();
            checkRun.Name.Should().Be("MSBuildLog Analyzer");
            checkRun.Title.Should().Be("MSBuildLog Analysis");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        [Fact]
        public void ShouldCreateCheckRunTestConsoleApp1Error()
        {
            var annotations = new[]
            {
                new Annotation(
                    "TestConsoleApp1\\Program.cs",
                    CheckWarningLevel.Failure, "CS1002",
                    "; expected",
                    13, 13)
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var resourcePath = TestUtils.GetResourcePath("testconsoleapp1-1error.binlog");

            var checkRun = GetCheckRun(new BinaryLogProcessor(TestLogger.Create<BinaryLogProcessor>(_testOutputHelper)), resourcePath, cloneRoot);

            checkRun.Success.Should().BeFalse();
            checkRun.Name.Should().Be("MSBuildLog Analyzer");
            checkRun.Title.Should().Be("MSBuildLog Analysis");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        [Fact]
        public void ShouldCreateCheckRunTestConsoleApp1CodeAnalysis()
        {
            var annotations = new[]
            {
                new Annotation(
                    "TestConsoleApp1\\Program.cs",
                    CheckWarningLevel.Warning, "CA2213",
                    "Microsoft.Usage : 'Program.MyClass' contains field 'Program.MyClass._inner' that is of IDisposable type: 'Program.MyOTherClass'. Change the Dispose method on 'Program.MyClass' to call Dispose or Close on this field.",
                    20, 20)
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var resourcePath = TestUtils.GetResourcePath("testconsoleapp1-codeanalysis.binlog");

            var checkRun = GetCheckRun(new BinaryLogProcessor(TestLogger.Create<BinaryLogProcessor>(_testOutputHelper)), resourcePath, cloneRoot);

            checkRun.Success.Should().BeTrue();
            checkRun.Name.Should().Be("MSBuildLog Analyzer");
            checkRun.Title.Should().Be("MSBuildLog Analysis");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        private CreateCheckRun GetCheckRun(IBinaryLogProcessor binaryLogProcessor, string inputFile = null, string cloneRoot = null)
        {
            inputFile = inputFile ?? Faker.System.FilePath();
            cloneRoot = cloneRoot ?? Faker.System.DirectoryPath();

            var outputFile = Faker.System.FilePath();

            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile(inputFile, new MockFileData(string.Empty, Encoding.UTF8));
            mockFileSystem.AddDirectory(Path.GetDirectoryName(outputFile));

            var buildLogProcessor = new BuildLogProcessor(mockFileSystem, binaryLogProcessor,
                TestLogger.Create<BuildLogProcessor>(_testOutputHelper));

            buildLogProcessor.Proces(inputFile, outputFile, cloneRoot);

            mockFileSystem.FileExists(outputFile).Should().BeTrue();

            var output = mockFileSystem.GetFile(outputFile).TextContents;
            output.Should().NotBeNullOrWhiteSpace();

            return JsonConvert.DeserializeObject<CreateCheckRun>(output);
        }

        private static IBinaryLogProcessor CreateMockBinaryLogProcessor(Annotation[] annotations)
        {
            var binaryLogProcessor = Substitute.For<IBinaryLogProcessor>();
            binaryLogProcessor.ProcessLog(Arg.Any<string>(), Arg.Any<string>())
                .Returns(annotations);
            return binaryLogProcessor;
        }
    }
}