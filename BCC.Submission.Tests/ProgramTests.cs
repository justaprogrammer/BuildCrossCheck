using BCC.Submission.Interfaces;
using Bogus;
using NSubstitute;
using Xunit;

namespace BCC.Submission.Tests
{
    public class ProgramTests
    {
        private static readonly Faker Faker;

        static ProgramTests()
        {
            Faker = new Faker();
        }

        [Fact]
        public void ShouldNotCallForInvalidArguments()
        {
            var buildLogProcessor = Substitute.For<ISubmissionService>();
            var commandLineParser = Substitute.For<ICommandLineParser>();
            var program = new Program(commandLineParser, buildLogProcessor);

            program.Run(new string[0]);
            commandLineParser.Received(1).Parse(Arg.Any<string[]>());
            buildLogProcessor.DidNotReceive().SubmitAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void ShouldCallForValidArguments()
        {
            var buildLogProcessor = Substitute.For<ISubmissionService>();
            var commandLineParser = Substitute.For<ICommandLineParser>();
            var applicationArguments = new ApplicationArguments()
            {
                Token = Faker.Random.String(),
                InputFile = Faker.System.FilePath(),
                HeadSha = Faker.Random.String()
            };

            commandLineParser.Parse(Arg.Any<string[]>()).Returns(applicationArguments);

            var program = new Program(commandLineParser, buildLogProcessor);

            program.Run(new string[0]);
            buildLogProcessor.Received(1).SubmitAsync(applicationArguments.InputFile, applicationArguments.Token, applicationArguments.HeadSha);
        }
    }
}
