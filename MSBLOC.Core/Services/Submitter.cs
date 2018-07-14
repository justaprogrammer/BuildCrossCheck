using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;
using Octokit;

namespace MSBLOC.Core.Services
{
    public class Submitter
    {
        private ITokenGenerator TokenGenerator { get; }
        private ILogger<Submitter> Logger { get; }

        public Submitter(ITokenGenerator tokenGenerator, ILogger<Submitter> logger = null)
        {
            TokenGenerator = tokenGenerator;
            Logger = logger ?? new NullLogger<Submitter>();
        }

        public async Task Submit(string owner, string name, string headSha,
            string checkRunName, ParsedBinaryLog parsedBinaryLog)
        {
            var jwtToken = TokenGenerator.GetToken();
            var appClient = new GitHubClient(new ProductHeaderValue("MSBLOC"))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            var checkSuite = await appClient.Check.Suite.Create(owner, name, new NewCheckSuite(headSha));
            var checkRun = await appClient.Check.Run.Create(owner, name, new NewCheckRun(checkRunName, headSha));
        }

        private static CheckRunAnnotation CreateCheckRunAnnotation(BuildErrorEventArgs buildErrorEventArgs)
        {
            var endLine = buildErrorEventArgs.EndLineNumber;
            if (endLine == 0)
            {
                endLine = buildErrorEventArgs.LineNumber;
            }

            return new CheckRunAnnotation(buildErrorEventArgs.File, "", buildErrorEventArgs.LineNumber,
                endLine, CheckWarningLevel.Failure, buildErrorEventArgs.Message, buildErrorEventArgs.Code,
                string.Empty);
        }

        private static CheckRunAnnotation CreateCheckRunAnnotation(BuildWarningEventArgs buildWarningEventArgs)
        {
            var endLine = buildWarningEventArgs.EndLineNumber;
            if (endLine == 0)
            {
                endLine = buildWarningEventArgs.LineNumber;
            }

            return new CheckRunAnnotation(buildWarningEventArgs.File, "", buildWarningEventArgs.LineNumber,
                endLine, CheckWarningLevel.Warning, buildWarningEventArgs.Message, buildWarningEventArgs.Code,
                string.Empty);
        }
    }
}