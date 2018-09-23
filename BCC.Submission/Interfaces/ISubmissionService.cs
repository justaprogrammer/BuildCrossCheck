using System.Threading.Tasks;

namespace BCC.Submission.Console.Interfaces
{
    public interface ISubmissionService
    {
        Task<bool> SubmitAsync(string inputFile, string token, string headSha);
    }
}