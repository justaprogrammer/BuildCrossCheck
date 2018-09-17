using System.Threading.Tasks;

namespace MSBLOC.Submission.Console.Interfaces
{
    public interface ISubmissionService
    {
        Task Submit(string inputFile, string token, string headSha);
    }
}