using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSBLOC.Web.Interfaces;

namespace MSBLOC.Web.Services
{
    public class LocalTempFileService : ITempFileService
    {
        private readonly ILogger<LocalTempFileService> _logger;

        public LocalTempFileService(ILogger<LocalTempFileService> logger)
        {
            _logger = logger;
        }

        public async Task<string> CreateFromStreamAsync(Stream source)
        {
            var targetFilePath = Path.GetTempFileName();
            using (var targetStream = File.Create(targetFilePath))
            {
                await source.CopyToAsync(targetStream);

                _logger.LogInformation($"Copied the uploaded file '{targetFilePath}'");
            }

            return targetFilePath;
        }
    }
}
