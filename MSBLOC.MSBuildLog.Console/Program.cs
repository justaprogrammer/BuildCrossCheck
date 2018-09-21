using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
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
            var consoleLogger = new ConsoleLogger("Program", (s, level) => level >= LogLevel.Debug, false);
            
            var fileSystem = new FileSystem();
            var binaryLogProcessor = new BinaryLogProcessor(consoleLogger);
            var buildLogProcessor = new BuildLogProcessor(fileSystem, binaryLogProcessor);
            var commandLineParser = new CommandLineParser(System.Console.WriteLine);
            var program = new Program(commandLineParser, buildLogProcessor);
            return program.Run(args) ? 0 : 1;
        }

        public Program(ICommandLineParser commandLineParser, IBuildLogProcessor buildLogProcessor)
        {
            _commandLineParser = commandLineParser;
            _buildLogProcessor = buildLogProcessor;
        }

        public bool Run(string[] args)
        {
            try
            {
                var result = _commandLineParser.Parse(args);
                if (result != null)
                {
                    _buildLogProcessor.Proces(result.InputFile, result.OutputFile, result.CloneRoot);
                    return true;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}
