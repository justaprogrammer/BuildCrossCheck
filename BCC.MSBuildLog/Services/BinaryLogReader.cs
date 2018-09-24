extern alias StructuredLogger;
using System.Collections.Generic;
using StructuredLogger::Microsoft.Build.Logging;

namespace BCC.MSBuildLog.Services
{
    public class BinaryLogReader : IBinaryLogReader
    {
        public IEnumerable<Record> ReadRecords(string binLogPath)
        {
            return new BinaryLogReplayEventSource().ReadRecords(binLogPath);
        }
    }
}