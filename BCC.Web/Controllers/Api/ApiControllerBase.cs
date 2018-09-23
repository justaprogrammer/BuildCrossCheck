using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace BCC.Web.Controllers.Api
{
    public class ApiControllerBase : Controller
    {
        protected string RepositoryName => User.Claims.FirstOrDefault(c => c.Type == "urn:msbloc:repositoryName")?.Value;
        protected string RepositoryOwner => User.Claims.FirstOrDefault(c => c.Type == "urn:msbloc:repositoryOwner")?.Value;
    }
}