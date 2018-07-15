using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;
using Octokit;

namespace MSBLOC.Core.Services
{
    public class Submitter : ISubmitter
    {
        private ICheckRunsClient CheckRunsClient { get; }
        private ILogger<Submitter> Logger { get; }

        public Submitter(ICheckRunsClient checkRunsClient, ILogger<Submitter> logger = null)
        {
            CheckRunsClient = checkRunsClient;
            Logger = logger ?? new NullLogger<Submitter>();
        }

        public async Task<CheckRun> SubmitCheckRun(string owner, string name, string headSha,
            string checkRunName, ParsedBinaryLog parsedBinaryLog, string checkRunTitle, string checkRunSummary)
        {
            var newCheckRunAnnotations = new List<NewCheckRunAnnotation>();
            newCheckRunAnnotations.AddRange(parsedBinaryLog.Errors.Select(CreateNewCheckRunAnnotation));
            newCheckRunAnnotations.AddRange(parsedBinaryLog.Warnings.Select(CreateNewCheckRunAnnotation));

            var newCheckRun = new NewCheckRun(checkRunName, headSha)
            {
                Output = new NewCheckRunOutput(checkRunTitle, checkRunSummary)
                {
                    Annotations = newCheckRunAnnotations
                }
            };

            return await CheckRunsClient.Create(owner, name, newCheckRun);
        }

        private static NewCheckRunAnnotation CreateNewCheckRunAnnotation(BuildErrorEventArgs buildErrorEventArgs)
        {
            var endLine = buildErrorEventArgs.EndLineNumber;
            if (endLine == 0)
            {
                endLine = buildErrorEventArgs.LineNumber;
            }

            return new NewCheckRunAnnotation(buildErrorEventArgs.File, "", buildErrorEventArgs.LineNumber,
                endLine, CheckWarningLevel.Failure, buildErrorEventArgs.Message)
            {
                Title = buildErrorEventArgs.Code
            };
        }

        private static NewCheckRunAnnotation CreateNewCheckRunAnnotation(BuildWarningEventArgs buildWarningEventArgs)
        {
            var endLine = buildWarningEventArgs.EndLineNumber;
            if (endLine == 0)
            {
                endLine = buildWarningEventArgs.LineNumber;
            }

            return new NewCheckRunAnnotation(buildWarningEventArgs.File, "", buildWarningEventArgs.LineNumber,
                endLine, CheckWarningLevel.Warning, buildWarningEventArgs.Message)
            {
                Title = buildWarningEventArgs.Code
            };
        }
    }
}