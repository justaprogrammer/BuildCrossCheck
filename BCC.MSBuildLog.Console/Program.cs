using BCC.MSBuildLog.Console.Interfaces;
using BCC.MSBuildLog.Console.Services;

namespace BCC.MSBuildLog.Console
{
    public class Program
    {
        private readonly ICommandLineParser _commandLineParser;
        private readonly IBuildLogProcessor _buildLogProcessor;

        [ExcludeFromCodeCoverage]
        static int Main(string[] args)
        {
            var fileSystem = new FileSystem();
            var binaryLogProcessor = new BinaryLogProcessor();
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
