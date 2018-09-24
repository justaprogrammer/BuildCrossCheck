using System;
using BCC.Submission.Interfaces;
using Fclp;

namespace BCC.Submission.Services
{
    public class CommandLineParser: ICommandLineParser
    {
        private readonly FluentCommandLineParser<ApplicationArguments> _parser;

        public CommandLineParser(Action<string> helpCallback)
        {
            _parser = new FluentCommandLineParser<ApplicationArguments>();

            _parser.Setup(arg => arg.InputFile)
                .As('i', "input")
                .WithDescription("Input file")
                .Required();

            _parser.Setup(arg => arg.Token)
                .As('t', "token")
                .WithDescription("Token")
                .Required();

            _parser.Setup(arg => arg.HeadSha)
                .As('h', "headSha")
                .WithDescription("Head Sha")
                .Required();

            _parser.SetupHelp("?", "help")
                .WithHeader(typeof(CommandLineParser).Assembly.FullName)
                .Callback(helpCallback);
        }

        public ApplicationArguments Parse(string[] args)
        {
            var commandLineParserResult = _parser.Parse(args);
            if (commandLineParserResult.HelpCalled)
            {
                return null;
            }

            if (commandLineParserResult.EmptyArgs || commandLineParserResult.HasErrors)
            {
                _parser.HelpOption.ShowHelp(_parser.Options);
                return null;
            }

            return _parser.Object;
        }
    }
}