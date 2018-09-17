using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using MSBLOC.Submission.Console.Interfaces;
using RestSharp;

namespace MSBLOC.Submission.Console.Services
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

        public async Task Submit(string inputFile, string token, string headSha)
        {
            var request = new RestRequest()
            {
                AlwaysMultipartFormData = true,
                RequestFormat = DataFormat.Json,
            };
            
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddParameter("CommitSha", headSha, ParameterType.RequestBody);
            request.AddFile("LogFile", _fileSystem.File.ReadAllBytes(inputFile), "file.txt");

            await _restClient.ExecutePostTaskAsync(request)
                .ConfigureAwait(false);
        }
    }
}