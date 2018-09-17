using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Interfaces;
using MSBLOC.Web.Attributes;
using MSBLOC.Web.Extensions;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;

namespace MSBLOC.Web.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    public class CheckRunController : MultiPartFormControllerBase<CheckRunController>
    {
        private readonly ICheckRunSubmissionService _checkRunSubmissionService;

        public CheckRunController(ILogger<CheckRunController> logger, ITempFileService tempFileService, ICheckRunSubmissionService checkRunSubmissionService)
            : base(logger, tempFileService)
        {
            _checkRunSubmissionService = checkRunSubmissionService;
        }

        [HttpPost]
        [DisableFormValueModelBinding]
        [MultiPartFormBinding(typeof(LogUploadData))]
        [Route("upload")]
        [Produces("application/json")]
        public async Task<IActionResult> Upload()
        {
            if (!Request.IsMultipartContentType())
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            // Bind form data to a model
            var logUploadData = await GetModelAsync<LogUploadData>();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resourcePath = TempFileService.GetFilePath(logUploadData.LogFile);

            var checkRun = await _checkRunSubmissionService.SubmitAsync(
                RepositoryOwner,
                RepositoryName,
                logUploadData.CommitSha,
                resourcePath);

            return Json(checkRun);
        }
    }
}