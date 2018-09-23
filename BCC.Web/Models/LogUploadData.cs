using System.ComponentModel.DataAnnotations;
using BCC.Web.Attributes;

namespace BCC.Web.Models
{
    public class LogUploadData
    {
        [Required]
        public string CommitSha { get; set; }

        [Required]
        [FormFile]
        public string LogFile { get; set; }
    }
}
