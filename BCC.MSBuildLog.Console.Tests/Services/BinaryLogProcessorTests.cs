namespace BCC.MSBuildLog.Console.Tests.Services
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
            var annotations = ProcessLog("testconsoleapp1-1warning.binlog", cloneRoot);

            annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1\\Program.cs", 
                    CheckWarningLevel.Warning, "CS0219", 
                    "The variable 'hello' is assigned but its value is never used", 
                    13, 13)
            );
        }

        [Fact]
        public void ShouldTestConsoleApp1Error()
        {
            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var annotations = ProcessLog("testconsoleapp1-1error.binlog", cloneRoot);

            annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1\\Program.cs",
                    CheckWarningLevel.Failure, "CS1002", 
                    "; expected", 
                    13, 13)
            );
        }

        [Fact]
        public void ShouldTestConsoleApp1CodeAnalysis()
        {
            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var annotations = ProcessLog("testconsoleapp1-codeanalysis.binlog", cloneRoot);

            annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1\\Program.cs",
                    CheckWarningLevel.Warning, "CA2213",
                    "Microsoft.Usage : 'Program.MyClass' contains field 'Program.MyClass._inner' that is of IDisposable type: 'Program.MyOTherClass'. Change the Dispose method on 'Program.MyClass' to call Dispose or Close on this field.", 
                    20, 20)
            );
        }

        [Fact]
        public void ShouldMSBLOC()
        {
            var cloneRoot = @"C:\projects\msbuildlogoctokitchecker\";
            var annotations = ProcessLog("msbloc.binlog", cloneRoot);

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
        public void ShouldParseOctokitGraphQL()
        {
            var cloneRoot = @"C:\projects\octokit-graphql\";
            var annotations = ProcessLog("octokit.graphql.binlog", cloneRoot);

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
        public void ShouldParseDBATools()
        {
            var cloneRoot = @"c:\github\dbatools\bin\projects\dbatools\";
            var annotations = ProcessLog("dbatools.binlog", cloneRoot);

            annotations.Length.Should().Be(0);
        }

        [Fact]
        public void ShouldThrowWhenBuildPathOutisdeCloneRoot()
        {
            var invalidOperationException = Assert.Throws<InvalidOperationException>(() =>
            {
                ProcessLog("testconsoleapp1-1warning.binlog", @"C:\projects\testconsoleapp2\");
            });

            invalidOperationException.Message.Should().Be(@"FilePath `C:\projects\testconsoleapp1\TestConsoleApp1\Program.cs` is not a child of `C:\projects\testconsoleapp2\`");

            invalidOperationException = Assert.Throws<InvalidOperationException>(() =>
            {
                ProcessLog("testconsoleapp1-1error.binlog", @"C:\projects\testconsoleapp2\");
            });
            invalidOperationException.Message.Should().Be(@"FilePath `C:\projects\testconsoleapp1\TestConsoleApp1\Program.cs` is not a child of `C:\projects\testconsoleapp2\`");
        }

        private Annotation[] ProcessLog(string resourceName, string cloneRoot)
        {
            var resourcePath = TestUtils.GetResourcePath(resourceName);
            File.Exists(resourcePath).Should().BeTrue();

            var parser = new BinaryLogProcessor(TestLogger.Create<BinaryLogProcessor>(_testOutputHelper));
            return parser.ProcessLog(resourcePath, cloneRoot).ToArray();
        }
    }
}