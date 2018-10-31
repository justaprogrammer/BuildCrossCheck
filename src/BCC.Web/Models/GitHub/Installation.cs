using System.Collections.Generic;

namespace BCC.Web.Models.GitHub
{
    public class Installation
    {
        public long Id { get; set; }

        public IReadOnlyList<Repository> Repositories { get; set; }

        public string Login { get; set; }
    }
}