using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSBLOC.Web.Models
{
    public class UploadFormData
    {
        public string RepoName { get; set; }
        public string Branch { get; set; }
        public string Sha { get; set; }
    }
}
