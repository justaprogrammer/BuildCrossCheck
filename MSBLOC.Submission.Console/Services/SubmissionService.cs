using System;
using System.IO.Abstractions;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MSBLOC.Submission.Console.Interfaces;
using RestSharp;

namespace MSBLOC.Submission.Console.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly IFileSystem _fileSystem;
        private readonly IRestClient _restClient;
        private readonly ILogger _logger;

        public SubmissionService(IFileSystem fileSystem, IRestClient restClient, ILogger logger = null)
        {
            _restClient = restClient;
            _logger = logger ?? NullLogger.Instance;
            _fileSystem = fileSystem;
        }

        public async Task<bool> SubmitAsync(string inputFile, string token, string headSha)
        {
            _logger.LogInformation("Submitting inputFile:{0} headSha:{1}", inputFile, headSha);

            var exists = _fileSystem.File.Exists(inputFile);
            if (!exists)
            {
                throw new InvalidOperationException($"File `{inputFile}` does not exist.");
            }

            var request = new RestRequest("api/checkrun/upload")
            {
                AlwaysMultipartFormData = true,
                RequestFormat = DataFormat.Json,
            };
            
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddParameter("CommitSha", headSha, ParameterType.RequestBody);
            request.AddFile("LogFile", _fileSystem.File.ReadAllBytes(inputFile), "file.txt");

            var restResponse = await _restClient.ExecutePostTaskAsync(request)
                .ConfigureAwait(false);

            _logger.LogDebug("Rest Response: {0}", restResponse.StatusCode);

            return restResponse.StatusCode == HttpStatusCode.Accepted;
        }
    }
}