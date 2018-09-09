using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MoreLinq;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Interfaces.GitHub;
using MSBLOC.Core.Model;
using MSBLOC.Core.Model.Builds;
using MSBLOC.Core.Model.GitHub;
using MSBLOC.Core.Model.LogAnalyzer;

namespace MSBLOC.Core.Services
{
    /// <inheritdoc />
    public class BinaryLogAnalyzerService : IBinaryLogAnalyzerService
    {
        private const string CheckRunName = "MSBuildLog Analyzer";
        private const string CheckRunTitle = "MSBuildLog Analysis";

        private readonly IBinaryLogProcessor _binaryLogProcessor;
        private readonly IGitHubAppModelService _gitHubAppModelService;
        private readonly ILogger _logger;

        public BinaryLogAnalyzerService(
            IBinaryLogProcessor binaryLogProcessor,
            IGitHubAppModelService gitHubAppModelService,
            ILogger<BinaryLogAnalyzerService> logger)
        {
            _logger = logger;
            _binaryLogProcessor = binaryLogProcessor;
            _gitHubAppModelService = gitHubAppModelService;
        }

        /// <inheritdoc />
        public async Task<CheckRun> SubmitAsync(string owner, string repository, string sha, string cloneRoot,
            string resourcePath)
        {
            _logger.LogInformation("SubmitAsync owner:{0} repository:{1} sha:{2} cloneRoot:{3} resourcePath:{4}",
                owner, repository, sha, cloneRoot, resourcePath);

            var startedAt = DateTimeOffset.Now;

            CheckRun checkRun;

            try
            {
                var buildDetails = _binaryLogProcessor.ProcessLog(resourcePath, cloneRoot);

                var logAnalyzerConfiguration = await _gitHubAppModelService.GetLogAnalyzerConfigurationAsync(owner, repository, sha);

                var annotations = CreateAnnotations(buildDetails, logAnalyzerConfiguration);

                var success = (annotations?.All(annotation => annotation.CheckWarningLevel != CheckWarningLevel.Failure) ?? true);

                checkRun = await SubmitCheckRun(annotations,
                    owner,
                    repository,
                    sha,
                    CheckRunName,
                    CheckRunTitle,
                    "",
                    startedAt,
                    DateTimeOffset.Now, success).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                checkRun = await SubmitCheckRun(null,
                        owner,
                        repository,
                        sha,
                        CheckRunName,
                        CheckRunTitle,
                        ex.ToString(),
                        startedAt,
                        DateTimeOffset.Now, false)
                    .ConfigureAwait(false);
            }

            _logger.LogInformation($"CheckRun Created - {checkRun.Url}");

            return checkRun;
        }

        private Annotation[] CreateAnnotations(BuildDetails buildDetails, LogAnalyzerConfiguration logAnalyzerConfiguration)
        {
            var lookup = logAnalyzerConfiguration?.Rules?.ToLookup(rule => rule.Code);
            return buildDetails.BuildMessages
                .Select(buildMessage => CreateAnnotation(buildDetails, buildMessage, lookup))
                .Where(annotation => annotation != null)
                .ToArray();
        }

        private static Annotation CreateAnnotation(BuildDetails buildDetails, BuildMessage buildMessage, ILookup<string, LogAnalyzerRule> lookup)
        {
            var filename =
                buildDetails.SolutionDetails.GetProjectItemPath(buildMessage.ProjectFile, buildMessage.File)
                    .TrimStart('/');

            var logAnalyzerRule = lookup?[buildMessage.Code].FirstOrDefault();

            var checkWarningLevel = buildMessage.MessageLevel == BuildMessageLevel.Error
                ? CheckWarningLevel.Failure
                : CheckWarningLevel.Warning;

            if (logAnalyzerRule != null)
            {
                switch (logAnalyzerRule.ReportAs)
                {
                    case ReportAs.AsIs:
                        break;
                    case ReportAs.Ignore:
                        return null;
                    case ReportAs.Notice:
                        checkWarningLevel = CheckWarningLevel.Notice;
                        break;
                    case ReportAs.Warning:
                        checkWarningLevel = CheckWarningLevel.Warning;
                        break;
                    case ReportAs.Error:
                        checkWarningLevel = CheckWarningLevel.Failure;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new Annotation(filename,
                checkWarningLevel,
                buildMessage.Code,
                $"{buildMessage.Code}: {buildMessage.Message}",
                buildMessage.LineNumber,
                buildMessage.EndLineNumber);
        }

        protected async Task<CheckRun> SubmitCheckRun(Annotation[] annotations,
            string owner, string name, string headSha,
            string checkRunName, string checkRunTitle, string checkRunSummary,
            DateTimeOffset startedAt, DateTimeOffset completedAt, bool success)
        {
            var annotationBatches = annotations?.Batch(50).ToArray();

            var checkRun = await _gitHubAppModelService.CreateCheckRunAsync(owner, name, headSha, checkRunName,
                    checkRunTitle, checkRunSummary, success, annotationBatches?.FirstOrDefault()?.ToArray(), startedAt, completedAt)
                .ConfigureAwait(false);

            if (annotationBatches != null)
            {
                foreach (var annotationBatch in annotationBatches.Skip(1))
                {
                    await _gitHubAppModelService.UpdateCheckRunAsync(checkRun.Id, owner, name, headSha, checkRunTitle,
                        checkRunSummary,
                        annotationBatch.ToArray(), startedAt, completedAt).ConfigureAwait(false);
                }
            }

            return checkRun;
        }
    }
}