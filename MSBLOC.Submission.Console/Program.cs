using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using MSBLOC.Submission.Console.Interfaces;
using MSBLOC.Submission.Console.Services;
using RestSharp;

namespace MSBLOC.Submission.Console
{
    public class Program
    {
        private readonly ICommandLineParser _commandLineParser;
        private readonly ISubmissionService _submissionService;

        [ExcludeFromCodeCoverage]
        static int Main(string[] args)
        {
            var fileSystem = new FileSystem();
            var commandLineParser = new CommandLineParser(System.Console.WriteLine);
            var submissionService = new SubmissionService(fileSystem, new RestClient());
            var program = new Program(commandLineParser, submissionService);
            return program.Run(args) ? 0 : 1;
        }

        public Program(ICommandLineParser commandLineParser, ISubmissionService submissionService)
        {
            _commandLineParser = commandLineParser;
            _submissionService = submissionService;
        }

        public bool Run(string[] args)
        {
            try
            {
                var result = _commandLineParser.Parse(args);
                if (result != null)
                {
                    _submissionService.Submit(result.InputFile, result.Token, "asdf");
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
