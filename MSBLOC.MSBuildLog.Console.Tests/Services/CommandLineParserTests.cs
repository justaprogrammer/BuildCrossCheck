using Bogus;
using FluentAssertions;
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
        public void ShouldReturnParsedArguments()
        {
            var listener = Substitute.For<ICommandLineParserCallBackListener>();
            var commandLineParser = new CommandLineParser(listener.Callback);

            var inputPath = Faker.System.FilePath();
            var outputPath = Faker.System.FilePath();
            var applicationArguments = commandLineParser.Parse(new[]{"-i", $@"""{inputPath}""", "-o", $@"""{outputPath}"""});

            listener.DidNotReceive().Callback(Arg.Any<string>());

            applicationArguments.Should().NotBeNull();
            applicationArguments.InputFile.Should().Be(inputPath);
            applicationArguments.OutputFile.Should().Be(outputPath);

            applicationArguments = commandLineParser.Parse(new[]{"--input", $@"""{inputPath}""", "--output", $@"""{outputPath}"""});

            listener.DidNotReceive().Callback(Arg.Any<string>());

            applicationArguments.Should().NotBeNull();
            applicationArguments.InputFile.Should().Be(inputPath);
            applicationArguments.OutputFile.Should().Be(outputPath);
        }

        public interface ICommandLineParserCallBackListener
        {
            void Callback(string obj);
        }
    }
}