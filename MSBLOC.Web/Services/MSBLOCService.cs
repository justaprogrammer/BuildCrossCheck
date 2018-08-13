using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitHubJwt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Services;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;
using Octokit;

namespace MSBLOC.Web.Services
{
    public class MSBLOCService : IMSBLOCService
    {
        private readonly IBinaryLogProcessor _binaryLogProcessor;
        private readonly Func<string, Task<ICheckRunSubmitter>> _checkRunSubmitterFactoryAsync;
        private readonly ITempFileService _tempFileService;
        private readonly ILogger _logger;

        public MSBLOCService(
            IBinaryLogProcessor binaryLogProcessor,
            Func<string, Task<ICheckRunSubmitter>> checkRunSubmitterFactoryAsync, 
            ITempFileService tempFileService,
            ILogger<MSBLOCService> logger = null)
        {
            _logger = logger;
            _binaryLogProcessor = binaryLogProcessor;
            _checkRunSubmitterFactoryAsync = checkRunSubmitterFactoryAsync;
            _tempFileService = tempFileService;
        }

        public async Task<CheckRun> SubmitAsync(string repositoryOwner, string repositoryName, SubmissionData submissionData)
        {
            var cloneRoot = submissionData.CloneRoot;
            var sha = submissionData.CommitSha;

            var resourcePath = _tempFileService.GetFilePath(submissionData.BinaryLogFile);

            var startedAt = DateTimeOffset.Now;

            var buildDetails = _binaryLogProcessor.ProcessLog(resourcePath, cloneRoot);

            var submitter = await _checkRunSubmitterFactoryAsync(repositoryOwner);

            var checkRun = await submitter.SubmitCheckRun(buildDetails: buildDetails,
                owner: repositoryOwner,
                name: repositoryName,
                headSha: sha,
                checkRunName: "MSBuildLog Analyzer",
                checkRunTitle: "MSBuildLog Analysis",
                checkRunSummary: "",
                startedAt: startedAt,
                completedAt: DateTimeOffset.Now);

            _logger.LogInformation($"CheckRun Created - {checkRun.HtmlUrl}");

            return checkRun;
        }
    }
}
