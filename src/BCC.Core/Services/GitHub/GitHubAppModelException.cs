using System;
using System.Runtime.Serialization;

namespace BCC.Core.Services.GitHub
{
    [Serializable]
    public class GitHubAppModelException : Exception
    {
        public GitHubAppModelException()
        {
        }

        public GitHubAppModelException(string message) : base(message)
        {
        }

        public GitHubAppModelException(string message, Exception inner) : base(message, inner)
        {
        }

        protected GitHubAppModelException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}