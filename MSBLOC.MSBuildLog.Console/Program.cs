using System.Diagnostics.CodeAnalysis;
using MSBLOC.MSBuildLog.Console.Interfaces;
using MSBLOC.MSBuildLog.Console.Services;

namespace MSBLOC.MSBuildLog.Console
{
    public class Program
    {
        private readonly ICommandLineParser _commandLineParser;
        private readonly IBuildLogProcessor _buildLogProcessor;

        [ExcludeFromCodeCoverage]
        static int Main(string[] args)
        {
            var buildLogProcessor = new BuildLogProcessor();
            var commandLineParser = new CommandLineParser(System.Console.WriteLine);
            var program = new Program(commandLineParser, buildLogProcessor);
            return 0;
        }

        public Program(ICommandLineParser commandLineParser, IBuildLogProcessor buildLogProcessor)
        {
            _commandLineParser = commandLineParser;
            _buildLogProcessor = buildLogProcessor;
        }

        public bool Run(string[] args)
        {
            var result = _commandLineParser.Parse(args);
            if (result != null)
            {
                _buildLogProcessor.Proces(result.InputFile, result.OutputFile);
                return true;
            }

            return false;
        }
    }
}
