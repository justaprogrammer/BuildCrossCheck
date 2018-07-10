using System;

namespace MSBLOC.Core.Models
{
    public class StubAnnotation: IEquatable<StubAnnotation>
    {
        public string FileName { get; set; }
        public string BlobHref { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string WarningLevel { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }
        public string RawDetails { get; set; }

        public bool Equals(StubAnnotation other)
        {
            if (other == null) return false;
                
            return StartLine == other.StartLine 
                   && EndLine == other.EndLine
                   && string.Equals(FileName, other.FileName)
                   && string.Equals(BlobHref, other.BlobHref)
                   && string.Equals(WarningLevel, other.WarningLevel) 
                   && string.Equals(Message, other.Message) 
                   && string.Equals(Title, other.Title) 
                   && string.Equals(RawDetails, other.RawDetails);
        }
    }
}