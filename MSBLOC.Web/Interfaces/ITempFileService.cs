using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MSBLOC.Web.Interfaces
{
    public interface ITempFileService
    {
        Task<string> CreateFromStreamAsync(Stream source);
    }
}
