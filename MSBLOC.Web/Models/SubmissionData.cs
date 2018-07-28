using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSBLOC.Web.Models
{
    public class SubmissionData
    {
        public string ApplicationOwner { get; set; }
        public string ApplicationName { get; set; }
        public string CommitSha { get; set; }
        public string CloneRoot { get; set; }
        public string BinaryLogFileName { get; set; }

        public override bool Equals(object obj)
        {
            return obj is SubmissionData data &&
                   ApplicationOwner == data.ApplicationOwner &&
                   ApplicationName == data.ApplicationName &&
                   CommitSha == data.CommitSha &&
                   CloneRoot == data.CloneRoot &&
                   BinaryLogFileName == data.BinaryLogFileName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ApplicationOwner, ApplicationName, CommitSha, CloneRoot, BinaryLogFileName);
        }
    }
}
