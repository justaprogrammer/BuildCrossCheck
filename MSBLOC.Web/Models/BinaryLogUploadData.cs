using System.ComponentModel.DataAnnotations;
using MSBLOC.Web.Attributes;

namespace MSBLOC.Web.Models
{
    public class BinaryLogUploadData
    {
        [Required]
        public string CommitSha { get; set; }

        [Required]
        public string CloneRoot { get; set; }

        [Required]
        [FormFile]
        public string BinaryLogFile { get; set; }
    }
}
