using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private readonly ILogAnalyzerService _logAnalyzerService;

        public BinaryLogController(ILogger<BinaryLogController> logger, ITempFileService tempFileService, ILogAnalyzerService logAnalyzerService)
            : base(logger, tempFileService)
        {
            _logAnalyzerService = logAnalyzerService;
        }

        [HttpPost]
        [DisableFormValueModelBinding]
        [MultiPartFormBinding(typeof(BinaryLogUploadData))]
        [Route("upload")]
        [Produces("application/json")]
        public async Task<IActionResult> Upload()
        {
            if (!Request.IsMultipartContentType())
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            // Bind form data to a model
            var binaryLogUploadData = new BinaryLogUploadData();

            var accumulator = await BuildMultiPartFormAccumulator<BinaryLogUploadData>();
            var bindingSuccessful = await BindDataAsync(binaryLogUploadData, accumulator.GetResults());

            if (!bindingSuccessful || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var requiredFormFileProperties = FormFileAttributeHelper.GetRequiredFormFileProperties(typeof(BinaryLogUploadData));
            foreach (var requiredFormFileProperty in requiredFormFileProperties)
            {
                var fileName = requiredFormFileProperty.GetValue(binaryLogUploadData);
                if (!TempFileService.Files.Contains(fileName))
                {
                    ModelState.AddModelError(requiredFormFileProperty.Name, $"File '{requiredFormFileProperty.Name}' with name: '{fileName}' not found in request.");
                    return BadRequest(ModelState);
                }
            }

            var checkRun = await _logAnalyzerService.SubmitAsync(
                RepositoryOwner,
                RepositoryName,
                binaryLogUploadData.CommitSha,
                binaryLogUploadData.CloneRoot,
                TempFileService.GetFilePath(binaryLogUploadData.BinaryLogFile));

            return Json(checkRun);
        }

        protected virtual Task<bool> BindDataAsync(BinaryLogUploadData model, Dictionary<string, StringValues> dataToBind)
        {
            var formValueProvider = new FormValueProvider(BindingSource.Form, new FormCollection(dataToBind), CultureInfo.CurrentCulture);
            return TryUpdateModelAsync(model, string.Empty, formValueProvider);
        }
    }
}
