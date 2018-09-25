using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using BCC.Core.Model.CheckRunSubmission;
using BCC.Core.Tests.Util;
using BCC.MSBuildLog.Interfaces;
using BCC.MSBuildLog.Model;
using BCC.MSBuildLog.Services;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace BCC.MSBuildLog.Tests.Services
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
        public void Should_Create_Empty_CheckRun()
        {
            var annotations = new Annotation[0];

            var checkRun = GetCheckRun(CreateMockBinaryLogProcessor(annotations));

            checkRun.Success.Should().BeTrue();
            checkRun.Name.Should().Be("MSBuild Log");
            checkRun.Title.Should().Be("0 Errors; 0 Warnings");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().AllBeEquivalentTo(annotations);
        }

        [Fact]
        public void Should_Throw_If_Configuration_Does_Not_Exists()
        {
            var annotations = new Annotation[0];

            var configurationFile = Faker.System.FilePath();
            var mockFileSystem = new MockFileSystem();
            new Action(() =>
                    GetCheckRun(CreateMockBinaryLogProcessor(annotations),configurationFile: configurationFile, mockFileSystem: mockFileSystem))
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Configuration file `" + configurationFile + "` does not exist.");
        }

        [Fact]
        public void Should_Throw_If_Configuration_Is_Null_Or_Whitespace()
        {
            var annotations = new Annotation[0];

            var configurationFile = Faker.System.FilePath();
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile(configurationFile, new MockFileData(string.Empty));

            new Action(() =>
                    GetCheckRun(CreateMockBinaryLogProcessor(annotations), configurationFile: configurationFile, mockFileSystem: mockFileSystem))
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Content of configuration file `" + configurationFile + "` is null or empty.");
        }

        [Fact]
        public void Should_Create_Empty_CheckRun_With_Configuration()
        {
            var annotations = new Annotation[0];

            var configurationFile = Faker.System.FilePath();
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile(configurationFile, new MockFileData("{rules: [{code: 'CS1234', reportAs: 'Ignore'}]}"));

            var checkRun = GetCheckRun(CreateMockBinaryLogProcessor(annotations), configurationFile: configurationFile, mockFileSystem: mockFileSystem);

            checkRun.Success.Should().BeTrue();
            checkRun.Name.Should().Be("MSBuild Log");
            checkRun.Title.Should().Be("0 Errors; 0 Warnings");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().AllBeEquivalentTo(annotations);
        }

        [Fact]
        public void Should_Create_CheckRun_With_Warning()
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
            checkRun.Name.Should().Be("MSBuild Log");
            checkRun.Title.Should().Be("0 Errors; 0 Warnings");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        [Fact]
        public void Should_Create_CheckRun_With_Configuration()
        {
            var annotations = new[]
            {
                new Annotation(
                    Faker.System.FilePath(),
                    CheckWarningLevel.Warning,
                    Faker.Lorem.Word(), Faker.Lorem.Word(),
                    Faker.Random.Int(), Faker.Random.Int())
            };

            var expectedCheckRunConfiguration = new CheckRunConfiguration
            {
                Name = Faker.Lorem.Word(),
                Rules = new[]
                {
                    new LogAnalyzerRule
                    {
                        Code = Faker.Lorem.Word(),
                        ReportAs = Faker.Random.Enum<ReportAs>()
                    },
                }
            };

            var configurationFile = Faker.System.FilePath();
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile(configurationFile, new MockFileData(JsonConvert.SerializeObject(expectedCheckRunConfiguration)));

            var mockBinaryLogProcessor = CreateMockBinaryLogProcessor(annotations);
            var checkRun = GetCheckRun(mockBinaryLogProcessor, configurationFile: configurationFile, mockFileSystem: mockFileSystem);

            mockBinaryLogProcessor.Received(1).ProcessLog(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CheckRunConfiguration>());
            var checkRunConfiguration = mockBinaryLogProcessor.ReceivedCalls().First().GetArguments()[2] as CheckRunConfiguration;
            checkRunConfiguration.Should().BeEquivalentTo(expectedCheckRunConfiguration);

            checkRun.Success.Should().BeTrue();
            checkRun.Name.Should().Be(expectedCheckRunConfiguration.Name);
            checkRun.Title.Should().Be("0 Errors; 0 Warnings");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        [Fact]
        public void Should_Create_CheckRun_With_Failure()
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
            checkRun.Name.Should().Be("MSBuild Log");
            checkRun.Title.Should().Be("0 Errors; 0 Warnings");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        [Fact]
        public void Should_Create_CheckRun_With_WarningAndFailure()
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
            checkRun.Name.Should().Be("MSBuild Log");
            checkRun.Title.Should().Be("0 Errors; 0 Warnings");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        [Fact]
        public void Should_Create_CheckRun_TestConsoleApp1_Warning()
        {
            var annotations = new[]
            {
                new Annotation(
                    "TestConsoleApp1/Program.cs",
                    CheckWarningLevel.Warning, "CS0219",
                    "The variable 'hello' is assigned but its value is never used",
                    13, 13)
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var resourcePath = TestUtils.GetResourcePath("testconsoleapp1-1warning.binlog");

            var checkRun = GetCheckRun(new BinaryLogProcessor(new BinaryLogReader(), TestLogger.Create<BinaryLogProcessor>(_testOutputHelper)), resourcePath, cloneRoot);

            checkRun.Success.Should().BeTrue();
            checkRun.Name.Should().Be("MSBuild Log");
            checkRun.Title.Should().Be("0 Errors; 1 Warning");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        [Fact]
        public void Should_Create_CheckRun_TestConsoleApp1_Error()
        {
            var annotations = new[]
            {
                new Annotation(
                    "TestConsoleApp1/Program.cs",
                    CheckWarningLevel.Failure, "CS1002",
                    "; expected",
                    13, 13)
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var resourcePath = TestUtils.GetResourcePath("testconsoleapp1-1error.binlog");

            var checkRun = GetCheckRun(new BinaryLogProcessor(new BinaryLogReader(), TestLogger.Create<BinaryLogProcessor>(_testOutputHelper)), resourcePath, cloneRoot);

            checkRun.Success.Should().BeFalse();
            checkRun.Name.Should().Be("MSBuild Log");
            checkRun.Title.Should().Be("1 Error; 0 Warnings");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        [Fact]
        public void Should_Create_CheckRun_TestConsoleApp1_CodeAnalysis()
        {
            var annotations = new[]
            {
                new Annotation(
                    "TestConsoleApp1/Program.cs",
                    CheckWarningLevel.Warning, "CA2213",
                    "Microsoft.Usage : 'Program.MyClass' contains field 'Program.MyClass._inner' that is of IDisposable type: 'Program.MyOTherClass'. Change the Dispose method on 'Program.MyClass' to call Dispose or Close on this field.",
                    20, 20)
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var resourcePath = TestUtils.GetResourcePath("testconsoleapp1-codeanalysis.binlog");

            var checkRun = GetCheckRun(new BinaryLogProcessor(new BinaryLogReader(), TestLogger.Create<BinaryLogProcessor>(_testOutputHelper)), resourcePath, cloneRoot);

            checkRun.Success.Should().BeTrue();
            checkRun.Name.Should().Be("MSBuild Log");
            checkRun.Title.Should().Be("0 Errors; 1 Warning");
            checkRun.Summary.Should().Be(string.Empty);
            checkRun.Annotations.Should().BeEquivalentTo<Annotation>(annotations);
        }

        private CreateCheckRun GetCheckRun(IBinaryLogProcessor binaryLogProcessor, 
            string inputFile = null, 
            string cloneRoot = null, 
            MockFileSystem mockFileSystem = null,
            string configurationFile = null)
        {
            inputFile = inputFile ?? Faker.System.FilePath();
            cloneRoot = cloneRoot ?? Faker.System.DirectoryPath();

            var outputFile = Faker.System.FilePath();

            mockFileSystem = mockFileSystem ?? new MockFileSystem();
            mockFileSystem.AddFile(inputFile, new MockFileData(string.Empty, Encoding.UTF8));
            mockFileSystem.AddDirectory(Path.GetDirectoryName(outputFile));

            var buildLogProcessor = new BuildLogProcessor(mockFileSystem, binaryLogProcessor,
                TestLogger.Create<BuildLogProcessor>(_testOutputHelper));

            buildLogProcessor.Proces(inputFile, outputFile, cloneRoot, configurationFile);

            mockFileSystem.FileExists(outputFile).Should().BeTrue();

            var output = mockFileSystem.GetFile(outputFile).TextContents;
            output.Should().NotBeNullOrWhiteSpace();

            return JsonConvert.DeserializeObject<CreateCheckRun>(output);
        }

        private static IBinaryLogProcessor CreateMockBinaryLogProcessor(Annotation[] annotations, int warningCount = 0, int errorCount = 0)
        {
            var binaryLogProcessor = Substitute.For<IBinaryLogProcessor>();
            binaryLogProcessor.ProcessLog(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CheckRunConfiguration>())
                .Returns(new LogData()
                {
                    Annotations = annotations,
                    WarningCount = warningCount,
                    ErrorCount = errorCount
                });
            return binaryLogProcessor;
        }
    }
}