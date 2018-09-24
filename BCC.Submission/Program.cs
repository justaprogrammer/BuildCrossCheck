using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using BCC.Submission.Interfaces;
using BCC.Submission.Services;
using RestSharp;

namespace BCC.Submission
{
    public class Program
    {
        private readonly ICommandLineParser _commandLineParser;
        private readonly ISubmissionService _submissionService;

        [ExcludeFromCodeCoverage]
        static int Main(string[] args)
        {
            var commandLineParser = new CommandLineParser(System.Console.WriteLine);
            var fileSystem = new FileSystem();
            var baseUrl = Environment.GetEnvironmentVariable("BCC_URL") ?? "https://buildcrosscheck.azurewebsites.net";
            var restClient = new RestClient(baseUrl);
            var submissionService = new SubmissionService(fileSystem, restClient);
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
