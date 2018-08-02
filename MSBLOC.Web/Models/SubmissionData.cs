using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;
using MSBLOC.Web.Attributes;

namespace MSBLOC.Web.Models
{
    public class SubmissionData
    {
        [Required]
        public string RepoOwner { get; set; }
        [Required]
        public string RepoName { get; set; }
        [Required]
        public string CommitSha { get; set; }
        [Required]
        public string CloneRoot { get; set; }
        [Required]
        [FormFile]
        public string BinaryLogFile { get; set; }
    }
}
