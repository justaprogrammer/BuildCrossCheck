using System;
using System.IO.Abstractions;
using System.Net;
using System.Threading.Tasks;
using BCC.Submission.Interfaces;
using RestSharp;

namespace BCC.Submission.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly IFileSystem _fileSystem;
        private readonly IRestClient _restClient;

        public SubmissionService(IFileSystem fileSystem, IRestClient restClient)
        {
            _restClient = restClient;
            _fileSystem = fileSystem;
        }

        public async Task<bool> SubmitAsync(string inputFile, string token, string headSha)
        {
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

            return restResponse.StatusCode == HttpStatusCode.OK;
        }
    }
}