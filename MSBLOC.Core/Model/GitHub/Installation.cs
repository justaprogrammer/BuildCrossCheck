using System.Collections.Generic;

namespace MSBLOC.Core.Model.GitHub
{
    public class Installation
    {
        public long Id { get; set; }

        public IReadOnlyList<Repository> Repositories { get; set; }

        public string Login { get; set; }
    }
}