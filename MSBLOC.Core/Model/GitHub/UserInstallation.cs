using System.Collections.Generic;

namespace MSBLOC.Core.Model.GitHub
{
    public class UserInstallation
    {
        public long Id { get; set; }

        public IReadOnlyList<UserRepository> Repositories { get; set; }

        public string Login { get; set; }
    }
}