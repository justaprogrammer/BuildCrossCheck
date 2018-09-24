using System.Collections.Generic;
using BCC.Core.Model.CheckRunSubmission;
using BCC.MSBuildLog.Model;
using JetBrains.Annotations;

namespace BCC.MSBuildLog.Interfaces
{
    /// <summary>
    /// This service reads a binary log file and outputs captured information.
    /// </summary>
    public interface IBinaryLogProcessor
    {
        /// <summary>
        /// Reads a binary log file and outputs captured information.
        /// </summary>
        /// <param name="binLogPath">The location of the (.binlog) file. Binary log files always have the extension binlog. MSBuild won't let you do it any other way.</param>
        /// <param name="cloneRoot">The location that the build was performed from. This assumes that the build path was a child of cloneRoot.</param>
        /// <param name="configuration">The check run configuration to follow.</param>
        /// <returns>A BuildDetails object.</returns>
        IReadOnlyCollection<Annotation> CreateAnnotations([NotNull] string binLogPath, [NotNull] string cloneRoot, CheckRunConfiguration configuration = null);
    }
}