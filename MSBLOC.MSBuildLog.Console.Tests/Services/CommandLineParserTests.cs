using Bogus;
using FluentAssertions;
using MSBLOC.MSBuildLog.Console.Services;
using NSubstitute;
using Xunit;

namespace MSBLOC.MSBuildLog.Console.Tests.Services
{
    public class CommandLineParserTests
    {
        private static readonly Faker Faker;

        static CommandLineParserTests()
        {
            Faker = new Faker();
        }

        [Fact]
        public void ShouldCallForHelpIfNothingSent()
        {
            var listener = Substitute.For<ICommandLineParserCallBackListener>();
            var commandLineParser = new CommandLineParser(listener.Callback);
            commandLineParser.Parse(new string[0]);

            listener.Received(1).Callback(Arg.Any<string>());
        }

        [Fact]
        public void ShouldRequireRequiredArguments()
        {
            var listener = Substitute.For<ICommandLineParserCallBackListener>();
            var commandLineParser = new CommandLineParser(listener.Callback);

            var inputPath = Faker.System.FilePath();
            var outputPath = Faker.System.FilePath();
            var cloneRoot = Faker.System.DirectoryPath();
            var applicationArguments = commandLineParser.Parse(new[]{"-i", $@"""{inputPath}""", "-o", $@"""{outputPath}""", "-c", $@"""{cloneRoot}"""});

            listener.DidNotReceive().Callback(Arg.Any<string>());

            applicationArguments.Should().NotBeNull();
            applicationArguments.InputFile.Should().Be(inputPath);
            applicationArguments.OutputFile.Should().Be(outputPath);
            applicationArguments.CloneRoot.Should().Be(cloneRoot);

            applicationArguments = commandLineParser.Parse(new[]{"--input", $@"""{inputPath}""", "--output", $@"""{outputPath}""", "--cloneRoot", $@"""{cloneRoot}""" });

            listener.DidNotReceive().Callback(Arg.Any<string>());

            applicationArguments.Should().NotBeNull();
            applicationArguments.InputFile.Should().Be(inputPath);
            applicationArguments.OutputFile.Should().Be(outputPath);
            applicationArguments.CloneRoot.Should().Be(cloneRoot);
        }

        public interface ICommandLineParserCallBackListener
        {
            void Callback(string obj);
        }
    }
}