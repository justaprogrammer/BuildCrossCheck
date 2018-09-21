using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
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
            var consoleLogger = new ConsoleLogger("Program", (s, level) => level >= LogLevel.Debug, false);

            var commandLineParser = new CommandLineParser(System.Console.WriteLine);
            var fileSystem = new FileSystem();
            var baseUrl = Environment.GetEnvironmentVariable("MSBLOC_URL") ?? "https://msblocweb.azurewebsites.net";
            var restClient = new RestClient(baseUrl);
            var submissionService = new SubmissionService(fileSystem, restClient, consoleLogger);
            var program = new Program(commandLineParser, submissionService);
            return program.Run(args) ? 0 : -1;
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
                    return _submissionService.SubmitAsync(result.InputFile, result.Token, result.HeadSha).Result;
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
