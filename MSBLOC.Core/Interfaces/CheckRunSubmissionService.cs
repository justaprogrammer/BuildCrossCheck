using System.Threading.Tasks;
using MSBLOC.Core.Model.GitHub;

namespace MSBLOC.Core.Interfaces
{
    /// <inheritdoc/>
    public class CheckRunSubmissionService : ICheckRunSubmissionService
    {
        /// <inheritdoc/>
        public Task<CheckRun> SubmitAsync(string owner, string repository, string sha, string cloneRoot, string resourcePath)
        {
            throw new System.NotImplementedException();
        }
    }
}