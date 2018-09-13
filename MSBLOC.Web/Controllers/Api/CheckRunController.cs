using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        public CheckRunController(ILogger<CheckRunController> logger, ITempFileService tempFileService)
            : base(logger, tempFileService)
        {
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

            throw new NotImplementedException();

//            var checkRun = await _binaryLogAnalyzerService.SubmitAsync(
//                RepositoryOwner,
//                RepositoryName,
//                logUploadData.CommitSha,
//                logUploadData.CloneRoot,
//                resourcePath);

//            return Json(checkRun);
        }
    }
}