using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MSBLOC.Web.Interfaces
{
    public interface ITempFileService : IDisposable
    {
        /// <summary>
        /// Creates a new temporary file from a stream.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="source">The source stream of the file.</param>
        /// <returns>The path to the temporary file.</returns>
        Task<string> CreateFromStreamAsync(string fileName, Stream source);

        /// <summary>
        /// Gets the path to a temporary file.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The path to the temporary file.</returns>
        string GetFilePath(string fileName);

        /// <summary>
        /// Gets a list of all temporary file names owned by this instance.
        /// </summary>
        /// <returns>The name of all temporary files owned by this instance.</returns>
        IEnumerable<string> Files { get; }
    }
}
