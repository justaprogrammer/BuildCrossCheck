using JetBrains.Annotations;
using MSBLOC.Core.Model;
using MSBLOC.Core.Model.Builds;

namespace MSBLOC.Core.Interfaces
{
    public interface IBinaryLogProcessor
    {
        /// <summary>
        /// Processes a msbuild binary log (.binlog) file for information that we can use to report on
        /// </summary>
        /// <param name="binLogPath">The location of the (.binlog) file. Binary log files always have the extension binlog. MSBuild won't let you do it any other way.</param>
        /// <param name="cloneRoot">The location that the build was performed from. This assumes that the build path was a child of cloneRoot.</param>
        /// <returns>A BuildDetails object.</returns>
        BuildDetails ProcessLog([NotNull] string binLogPath, [NotNull] string cloneRoot);
    }
}