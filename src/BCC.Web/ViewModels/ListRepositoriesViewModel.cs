using System.Linq;
using BCC.Infrastructure.Models;
using Repository = BCC.Web.Models.GitHub.Repository;

namespace BCC.Web.ViewModels
{
    public class ListRepositoriesViewModel: ViewModelBase
    {
        public IGrouping<string, Repository>[] RepositoriesByOwner { get; set; }
        public ILookup<long, AccessToken> TokenLookup { get; set; }
        public string CreatedToken { get; set; }
        public long? CreatedTokenRepoId { get; set; }
    }
}