using System;
using System.Collections.Generic;
using System.Text;
using MSBLOC.Infrastructure.Models;

namespace MSBLOC.Infrastructure.Interfaces
{
    public interface IAccessTokenRepository : IRepository<AccessToken, Guid>
    {
    }
}
