using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BCC.Core.Model.CheckRunSubmission;
using BCC.Core.Tests.Util;
using BCC.MSBuildLog.Model;
using BCC.MSBuildLog.Services;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace BCC.MSBuildLog.Tests.Services
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
        public void Should_TestConsoleApp1_Warning()
        {
            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var annotations = CreateAnnotations("testconsoleapp1-1warning.binlog", cloneRoot);

            annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1\\Program.cs", 
                    CheckWarningLevel.Warning, "CS0219", 
                    "The variable 'hello' is assigned but its value is never used", 
                    13, 13)
            );
        }

        [Fact]
        public void Should_TestConsoleApp1_Warning_ConfigureAs_Warning_For_Other_Code()
        {
            var checkRunConfiguration = new CheckRunConfiguration()
            {
                Rules = new LogAnalyzerRule[]
                {
                    new LogAnalyzerRule()
                    {
                        Code = "CS02191",
                        ReportAs = ReportAs.Error
                    }
                }
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var annotations = CreateAnnotations("testconsoleapp1-1warning.binlog", cloneRoot, checkRunConfiguration);

            annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1\\Program.cs",
                    CheckWarningLevel.Warning, "CS0219",
                    "The variable 'hello' is assigned but its value is never used",
                    13, 13)
            );
        }

        [Fact]
        public void Should_TestConsoleApp1_Warning_ConfigureAs_Warning()
        {
            var checkRunConfiguration = new CheckRunConfiguration()
            {
                Rules = new LogAnalyzerRule[]
                {
                    new LogAnalyzerRule()
                    {
                        Code = "CS0219",
                        ReportAs = ReportAs.Warning
                    }
                }
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var annotations = CreateAnnotations("testconsoleapp1-1warning.binlog", cloneRoot, checkRunConfiguration);

            annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1\\Program.cs",
                    CheckWarningLevel.Warning, "CS0219",
                    "The variable 'hello' is assigned but its value is never used",
                    13, 13)
            );
        }
         
        [Fact]
        public void Should_TestConsoleApp1_Warning_ConfigureAs_Notice()
        {
            var checkRunConfiguration = new CheckRunConfiguration()
            {
                Rules = new LogAnalyzerRule[]
                {
                    new LogAnalyzerRule()
                    {
                        Code = "CS0219",
                        ReportAs = ReportAs.Notice
                    }
                }
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var annotations = CreateAnnotations("testconsoleapp1-1warning.binlog", cloneRoot, checkRunConfiguration);

            annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1\\Program.cs",
                    CheckWarningLevel.Notice, "CS0219",
                    "The variable 'hello' is assigned but its value is never used",
                    13, 13)
            );
        }

        [Fact]
        public void Should_TestConsoleApp1_Warning_ConfigureAs_Error()
        {
            var checkRunConfiguration = new CheckRunConfiguration()
            {
                Rules = new LogAnalyzerRule[]
                {
                    new LogAnalyzerRule()
                    {
                        Code = "CS0219",
                        ReportAs = ReportAs.Error
                    }
                }
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var annotations = CreateAnnotations("testconsoleapp1-1warning.binlog", cloneRoot, checkRunConfiguration);

            annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1\\Program.cs",
                    CheckWarningLevel.Failure, "CS0219",
                    "The variable 'hello' is assigned but its value is never used",
                    13, 13)
            );
        }

        [Fact]
        public void Should_TestConsoleApp1_Warning_ConfigureAs_Ignore()
        {
            var checkRunConfiguration = new CheckRunConfiguration()
            {
                Rules = new LogAnalyzerRule[]
                {
                    new LogAnalyzerRule()
                    {
                        Code = "CS0219",
                        ReportAs = ReportAs.Ignore
                    }
                }
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var annotations = CreateAnnotations("testconsoleapp1-1warning.binlog", cloneRoot, checkRunConfiguration);
            annotations.Should().BeEmpty();
        }

        [Fact]
        public void Should_TestConsoleApp1_Error()
        {
            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var annotations = CreateAnnotations("testconsoleapp1-1error.binlog", cloneRoot);

            annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1\\Program.cs",
                    CheckWarningLevel.Failure, "CS1002", 
                    "; expected", 
                    13, 13)
            );
        }

        [Fact]
        public void Should_TestConsoleApp1_CodeAnalysis()
        {
            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var annotations = CreateAnnotations("testconsoleapp1-codeanalysis.binlog", cloneRoot);

            annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1\\Program.cs",
                    CheckWarningLevel.Warning, "CA2213",
                    "Microsoft.Usage : 'Program.MyClass' contains field 'Program.MyClass._inner' that is of IDisposable type: 'Program.MyOTherClass'. Change the Dispose method on 'Program.MyClass' to call Dispose or Close on this field.", 
                    20, 20)
            );
        }

        [Fact]
        public void Should_MSBLOC()
        {
            var cloneRoot = @"C:\projects\msbuildlogoctokitchecker\";
            var annotations = CreateAnnotations("msbloc.binlog", cloneRoot);

            annotations.Length.Should().Be(10);

            annotations[0].Should().BeEquivalentTo(
                new Annotation(
                    "MSBLOC.Core.Tests\\Services\\BinaryLogProcessorTests.cs", 
                    CheckWarningLevel.Warning,"CS0219",
                    "The variable 'filename' is assigned but its value is never used",
                    56, 56));

            annotations[1].Should().BeEquivalentTo(
                new Annotation(
                    "MSBLOC.Core.Tests\\Services\\BinaryLogProcessorTests.cs", 
                    CheckWarningLevel.Warning,"CS0219",
                    "The variable 'filename' is assigned but its value is never used",
                    83, 83));
        }

        [Fact]
        public void Should_Parse_OctokitGraphQL()
        {
            var cloneRoot = @"C:\projects\octokit-graphql\";
            var annotations = CreateAnnotations("octokit.graphql.binlog", cloneRoot);

            annotations.Length.Should().Be(803);

            annotations[0].Should().BeEquivalentTo(
                new Annotation(
                    "Octokit.GraphQL.Core\\Connection.cs",
                    CheckWarningLevel.Warning,"CS1591",
                    "Missing XML comment for publicly visible type or member 'Connection.Uri'",
                    43, 43));

            annotations[1].Should().BeEquivalentTo(
                new Annotation(
                    "Octokit.GraphQL.Core\\Connection.cs",
                    CheckWarningLevel.Warning,"CS1591",
                    "Missing XML comment for publicly visible type or member 'Connection.CredentialStore'",
                    44, 44));
        }

        [Fact]
        public void Should_Parse_DBATools()
        {
            var cloneRoot = @"c:\github\dbatools\bin\projects\dbatools\";
            var annotations = CreateAnnotations("dbatools.binlog", cloneRoot);

            annotations.Length.Should().Be(0);
        }

        [Fact]
        public void Should_ThrowWhen_BuildPath_Outisde_CloneRoot()
        {
            var invalidOperationException = Assert.Throws<InvalidOperationException>(() =>
            {
                CreateAnnotations("testconsoleapp1-1warning.binlog", @"C:\projects\testconsoleapp2\");
            });

            invalidOperationException.Message.Should().Be(@"FilePath `C:\projects\testconsoleapp1\TestConsoleApp1\Program.cs` is not a child of `C:\projects\testconsoleapp2\`");

            invalidOperationException = Assert.Throws<InvalidOperationException>(() =>
            {
                CreateAnnotations("testconsoleapp1-1error.binlog", @"C:\projects\testconsoleapp2\");
            });
            invalidOperationException.Message.Should().Be(@"FilePath `C:\projects\testconsoleapp1\TestConsoleApp1\Program.cs` is not a child of `C:\projects\testconsoleapp2\`");
        }

        private Annotation[] CreateAnnotations(string resourceName, string cloneRoot, CheckRunConfiguration checkRunConfiguration = null)
        {
            var resourcePath = TestUtils.GetResourcePath(resourceName);
            File.Exists(resourcePath).Should().BeTrue();

            var binaryLogReader = new BinaryLogReader();
            var parser = new BinaryLogProcessor(binaryLogReader, TestLogger.Create<BinaryLogProcessor>(_testOutputHelper));
            return parser.CreateAnnotations(resourcePath, cloneRoot, checkRunConfiguration).ToArray();
        }
    }
}