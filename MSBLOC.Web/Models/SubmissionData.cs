using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;
using MSBLOC.Web.Attributes;

namespace MSBLOC.Web.Models
{
    public class SubmissionData : SubmissionFormData
    {
        public string RepoOwner { get; set; }
        public string RepoName { get; set; }
    }
}
