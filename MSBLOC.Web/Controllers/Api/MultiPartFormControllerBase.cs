using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using MSBLOC.Web.Controllers.Extensions;
using MSBLOC.Web.Extensions;
using MSBLOC.Web.Helpers;
using MSBLOC.Web.Interfaces;

namespace MSBLOC.Web.Controllers.Api
{
    public abstract class MultiPartFormControllerBase<TController> : ApiControllerBase
    {
        private readonly FormOptions _defaultFormOptions = new FormOptions();

        protected ITempFileService TempFileService { get; }
        protected ILogger<TController> Logger { get; }

        protected MultiPartFormControllerBase(ILogger<TController> logger, ITempFileService tempFileService)
        {
            TempFileService = tempFileService;
            Logger = logger;
        }

        protected async Task<KeyValueAccumulator> BuildMultiPartFormAccumulator<TModel>() where TModel : class, new()
        {
            var httpContextRequest = HttpContext.Request;

            // Used to accumulate all the form url encoded key value pairs in the request
            var formAccumulator = new KeyValueAccumulator();

            var boundary = Request.GetBoundary(_defaultFormOptions);
            var reader = new MultipartReader(boundary, httpContextRequest.Body);

            Logger.LogDebug("Reading next section");

            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    Logger.LogDebug("Content Disposition Header: {0}", section.ContentDisposition);

                    if (contentDisposition.IsFileContentDisposition())
                    {
                        var fileName = contentDisposition.FileName.Value;

                        var formFileNames = FormFileAttributeHelper.GetFormFileNames(typeof(TModel));
                        if (!formFileNames.Contains(contentDisposition.Name.Value))
                        {
                            Logger.LogWarning($"Unknown file '{contentDisposition.Name.Value}' with fileName: '{fileName}' is being ignored.");
                            // Drains any remaining section body that has not been consumed and
                            // reads the headers for the next section.
                            section = await reader.ReadNextSectionAsync();
                            continue;
                        }

                        formAccumulator.Append(contentDisposition.Name.Value, fileName);

                        var path = await TempFileService.CreateFromStreamAsync(fileName, section.Body);

                        Logger.LogInformation($"Copied the uploaded file '{fileName}' to path: '{path}'");
                    }
                    else if (contentDisposition.IsFormDataContentDisposition())
                    {
                        // Content-Disposition: form-data; name="key"

                        // Do not limit the key name length here because the 
                        // multipart headers length limit is already in effect.
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);

                        Logger.LogDebug("Retrieving value for {0}", key);

                        var encoding = section.GetEncoding();
                        var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true);
                        using (streamReader)
                        {
                            // The value length limit is enforced by MultipartBodyLengthLimit
                            var value = await streamReader.ReadToEndAsync();
                            if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                value = string.Empty;
                            }

                            formAccumulator.Append(key.Value, value);

                            if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
                            {
                                throw new InvalidDataException(
                                    $"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
                            }
                        }
                    }
                }

                // Drains any remaining section body that has not been consumed and
                // reads the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            return formAccumulator;
        }

        protected void ValidateModel<TFormFileModel>(TFormFileModel logUploadData)
        {
            foreach (var requiredFormFileProperty in FormFileAttributeHelper.GetRequiredFormFileProperties(typeof(TFormFileModel)))
            {
                var fileName = requiredFormFileProperty.GetValue(logUploadData);
                if (!TempFileService.Files.Contains(fileName))
                {
                    ModelState.AddModelError(requiredFormFileProperty.Name,
                        $"File '{requiredFormFileProperty.Name}' with name: '{fileName}' not found in request.");
                }
            }
        }

        protected virtual Task<bool> BindModelAsync<TFormFileModel>(TFormFileModel model, Dictionary<string, StringValues> dataToBind) where TFormFileModel : class
        {
            var formValueProvider = new FormValueProvider(BindingSource.Form, new FormCollection(dataToBind), CultureInfo.CurrentCulture);
            return TryUpdateModelAsync(model, string.Empty, formValueProvider);
        }

        protected async Task<TFormFileModel> GetModelAsync<TFormFileModel>() where TFormFileModel : class, new()
        {
            var logUploadData = new TFormFileModel();
            var accumulator = await BuildMultiPartFormAccumulator<TFormFileModel>();
            var bindingSuccessful = await BindModelAsync(logUploadData, accumulator.GetResults());
            if (bindingSuccessful)
            {
                ValidateModel(logUploadData);
            }

            return logUploadData;
        }
    }
}