extern alias StructuredLogger;
using System.Collections.Generic;
using StructuredLogger::Microsoft.Build.Logging;

namespace BCC.MSBuildLog.Services
{
    public interface IBinaryLogReader
    {
        IEnumerable<Record> ReadRecords(string binLogPath);
    }
}