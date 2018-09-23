using System;
using BCC.MSBuildLog.Console.Interfaces;
using Fclp;

namespace BCC.MSBuildLog.Console.Services
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

            _parser.Setup(arg => arg.OutputFile)
                .As('o', "output")
                .WithDescription("Output file")
                .Required();

            _parser.Setup(arg => arg.CloneRoot)
                .As('c', "cloneRoot")
                .WithDescription("Clone root")
                .Required();

            _parser.SetupHelp("?", "help")
                .WithHeader("MSBuildLog.Console")
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