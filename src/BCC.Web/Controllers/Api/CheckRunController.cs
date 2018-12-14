﻿using System.Threading.Tasks;
using BCC.Web.Attributes;
using BCC.Web.Extensions;
using BCC.Web.Interfaces;
using BCC.Web.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ICheckRunSubmissionService = BCC.Web.Interfaces.ICheckRunSubmissionService;

namespace BCC.Web.Controllers.Api
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