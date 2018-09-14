using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MSBLOC.Core.Interfaces;
using MSBLOC.Web.Attributes;
using MSBLOC.Web.Extensions;
using MSBLOC.Web.Helpers;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;

namespace MSBLOC.Web.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    public class BinaryLogController : MultiPartFormControllerBase<BinaryLogController>
    {
        private readonly IBinaryLogAnalyzerService _binaryLogAnalyzerService;

        public BinaryLogController(ILogger<BinaryLogController> logger, ITempFileService tempFileService, IBinaryLogAnalyzerService binaryLogAnalyzerService)
            : base(logger, tempFileService)
        {
            _binaryLogAnalyzerService = binaryLogAnalyzerService;
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
            var checkRun = await _binaryLogAnalyzerService.SubmitAsync(
                RepositoryOwner,
                RepositoryName,
                logUploadData.CommitSha,
                logUploadData.CloneRoot,
                resourcePath);

            return Json(checkRun);
        }
    }
}
