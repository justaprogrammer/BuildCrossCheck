using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MoreLinq;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;
using MSBLOC.Core.Model.Builds;

namespace MSBLOC.Core.Services
{
    public class MSBLOCService : IMSBLOCService
    {
        private readonly IBinaryLogProcessor _binaryLogProcessor;
        private readonly IGitHubAppModelService _gitHubAppModelService;
        private readonly ILogger _logger;

        public MSBLOCService(
            IBinaryLogProcessor binaryLogProcessor,
            IGitHubAppModelService gitHubAppModelService,
            ILogger<MSBLOCService> logger)
        {
            _logger = logger;
            _binaryLogProcessor = binaryLogProcessor;
            _gitHubAppModelService = gitHubAppModelService;
        }

        public async Task<CheckRun> SubmitAsync(string repoOwner, string repoName, string sha, string cloneRoot,
            string resourcePath, int number)
        {
            var startedAt = DateTimeOffset.Now;

            var buildDetails = _binaryLogProcessor.ProcessLog(resourcePath, cloneRoot);

            var annotations = await CreateAnnotations(buildDetails, repoOwner, repoName, number);

            var checkRun = await SubmitCheckRun(annotations: annotations,
                owner: repoOwner,
                name: repoName,
                headSha: sha,
                checkRunName: "MSBuildLog Analyzer",
                checkRunTitle: "MSBuildLog Analysis",
                checkRunSummary: "",
                startedAt: startedAt,
                completedAt: DateTimeOffset.Now).ConfigureAwait(false);

            _logger.LogInformation($"CheckRun Created - {checkRun.Url}");

            return checkRun;
        }

        private async Task<Annotation[]> CreateAnnotations(BuildDetails buildDetails, string repoOwner, string repoName,
            int number)
        {
            await _gitHubAppModelService.GetPullRequestChangedPathsAsync(repoOwner, repoName, number);
            return new Annotation[0];
        }

        protected async Task<CheckRun> SubmitCheckRun(Annotation[] annotations,
            string owner, string name, string headSha,
            string checkRunName, string checkRunTitle, string checkRunSummary,
            DateTimeOffset startedAt, DateTimeOffset completedAt)
        {
            var annotationBatches = annotations?.Batch(50).ToArray();

            var checkRun = await _gitHubAppModelService.CreateCheckRunAsync(owner, name, headSha, checkRunName, checkRunTitle, checkRunSummary, annotationBatches?.FirstOrDefault()?.ToArray(), startedAt, completedAt).ConfigureAwait(false);

            foreach (var annotationBatch in annotationBatches.Skip(1))
            {
                await _gitHubAppModelService.UpdateCheckRunAsync(checkRun.Id, owner, name, headSha, checkRunTitle, checkRunSummary,
                    annotationBatch.ToArray(), startedAt, completedAt).ConfigureAwait(false);
            }

            return checkRun;
        }
    }
}
