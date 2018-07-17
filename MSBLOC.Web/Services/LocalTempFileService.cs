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

        private readonly IDictionary<string, string> _managedPathDictionary = new Dictionary<string, string>();

        public LocalTempFileService(ILogger<LocalTempFileService> logger)
        {
            _logger = logger;
        }

        public async Task<string> CreateFromStreamAsync(string fileName, Stream source)
        {
            var targetFilePath = Path.GetTempFileName();

            _managedPathDictionary.Add(fileName, targetFilePath);

            using (var targetStream = File.Create(targetFilePath))
            {
                await source.CopyToAsync(targetStream);

                _logger.LogInformation($"Copied the uploaded file '{fileName}' to path: '{targetFilePath}'");
            }

            return targetFilePath;
        }

        public void Dispose()
        {
            foreach (var kvp in _managedPathDictionary)
            {
                try
                {
                    File.Delete(kvp.Value);
                }
                catch(Exception e)
                {
                    _logger.LogError(e, $"Error deleting file '{kvp.Key}' with path: '{kvp.Value}'");
                }
            }
        }
    }
}
