using System.Collections.Generic;

namespace BCC.Core.Model.GitHub
{
    public class Installation
    {
        public long Id { get; set; }

        public IReadOnlyList<Repository> Repositories { get; set; }

        public string Login { get; set; }
    }
}