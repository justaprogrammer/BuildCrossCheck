using System;
using Bogus;
using MSBLOC.Submission.Console.Interfaces;
using NSubstitute;
using Xunit;

namespace MSBLOC.Submission.Console.Tests
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
            buildLogProcessor.DidNotReceive().Submit(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void ShouldCallForValidArguments()
        {
            var buildLogProcessor = Substitute.For<ISubmissionService>();
            var commandLineParser = Substitute.For<ICommandLineParser>();
            var applicationArguments = new ApplicationArguments()
            {
                Token = Faker.Random.String(),
                InputFile = Faker.System.FilePath()
            };

            commandLineParser.Parse(Arg.Any<string[]>()).Returns(applicationArguments);

            var program = new Program(commandLineParser, buildLogProcessor);

            program.Run(new string[0]);
            buildLogProcessor.Received(1).Submit(Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
