using Octokit;

namespace MSBLOC.Core.Model
{
    public class UserRepository
    {
        public string Owner { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public long Id { get; set; }
    }
}