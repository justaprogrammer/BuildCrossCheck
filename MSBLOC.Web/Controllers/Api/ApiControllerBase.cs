using System.Linq;
using Microsoft.Extensions.Logging;
using MSBLOC.Web.Interfaces;

namespace MSBLOC.Web.Controllers.Api
{
    public class ApiControllerBase : MultiPartFormControllerBase<BinaryLogController>
    {
        protected ApiControllerBase(ILogger<BinaryLogController> logger, ITempFileService tempFileService) : base(logger, tempFileService)
        {
        }

        protected string RepositoryName => User.Claims.FirstOrDefault(c => c.Type == "urn:msbloc:repositoryName")?.Value;
        protected string RepositoryOwner => User.Claims.FirstOrDefault(c => c.Type == "urn:msbloc:repositoryOwner")?.Value;
    }
}