using BCC.Web.Interfaces;

namespace BCC.Web.Services
{
    public class LocalTempFileService : ITempFileService
    {
        private readonly ILogger<LocalTempFileService> _logger;

        private readonly ConcurrentDictionary<string, string> _managedPathDictionary = new ConcurrentDictionary<string, string>();

        public LocalTempFileService(ILogger<LocalTempFileService> logger)
        {
            _logger = logger;
        }

        public async Task<string> CreateFromStreamAsync(string fileName, Stream source)
        {
            var targetFilePath = Path.GetTempFileName();

            _managedPathDictionary.AddOrUpdate(fileName, targetFilePath, (f, oldPath) =>
            {
                DeleteFile(f, oldPath);

                return targetFilePath;
            });

            using (var targetStream = File.Create(targetFilePath))
            {
                await source.CopyToAsync(targetStream);

                _logger.LogInformation($"Copied the uploaded file '{fileName}' to path: '{targetFilePath}'");
            }

            return targetFilePath;
        }

        public string GetFilePath(string fileName)
        {
            try
            {
                return _managedPathDictionary[fileName];
            }
            catch (Exception)
            {
                throw new FileNotFoundException($"File: '{fileName}' not found.");
            }
        }

        public IEnumerable<string> Files => _managedPathDictionary.Keys;

        public void Dispose()
        {
            foreach (var kvp in _managedPathDictionary)
            {
                DeleteFile(kvp.Key, kvp.Value);
            }
        }

        private void DeleteFile(string fileName, string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error deleting file '{fileName}' with path: '{filePath}'");
            }
        }
    }
}
