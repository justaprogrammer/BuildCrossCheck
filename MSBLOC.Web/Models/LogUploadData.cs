using System.ComponentModel.DataAnnotations;
using MSBLOC.Web.Attributes;

namespace MSBLOC.Web.Models
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
