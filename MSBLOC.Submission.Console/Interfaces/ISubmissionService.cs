using System.Threading.Tasks;

namespace MSBLOC.Submission.Console.Interfaces
{
    public interface ISubmissionService
    {
        Task<bool> SubmitAsync(string inputFile, string token, string headSha);
    }
}