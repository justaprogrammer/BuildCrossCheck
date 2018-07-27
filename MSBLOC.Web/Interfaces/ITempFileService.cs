using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MSBLOC.Web.Interfaces
{
    public interface ITempFileService : IDisposable
    {
        Task<string> CreateFromStreamAsync(string fileName, Stream source);
        string GetFilePath(string fileName);
        IEnumerable<string> Files { get; }
    }
}
