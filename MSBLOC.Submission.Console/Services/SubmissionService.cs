using System;
using System.IO.Abstractions;
using MSBLOC.Submission.Console.Interfaces;

namespace MSBLOC.Submission.Console.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly IFileSystem _fileSystem;

        public SubmissionService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void Submit(string inputFile, string token)
        {
            throw new NotImplementedException();
        }
    }
}