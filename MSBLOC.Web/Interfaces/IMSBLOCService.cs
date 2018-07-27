using System.Threading.Tasks;
using MSBLOC.Web.Models;
using Octokit;

namespace MSBLOC.Web.Interfaces
{
    public interface IMSBLOCService
    {
        Task<CheckRun> Submit(SubmitionData submitionData);
    }
}