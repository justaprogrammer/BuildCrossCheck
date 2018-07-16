using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MSBLOC.Web.Controllers.api;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Tests.Services;
using MSBLOC.Web.Tests.Util;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Web.Tests.Controllers.api
{
    public class FileControllerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<FileControllerTests> _logger;

        public FileControllerTests(ITestOutputHelper testOutputHelper)
        {
            _logger = TestLogger.Create<FileControllerTests>(testOutputHelper);
            _testOutputHelper = testOutputHelper;
        }

        private static string GetResourcePath(string file)
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            dirPath.Should().NotBeNull();
            return Path.Combine(dirPath, "Resources", file);
        }

        [Fact]
        public async Task UploadTest()
        {
            var fileContents = "This is some dummy file contents";

            var fileService = Substitute.For<ITempFileService>();
            fileService.CreateFromStreamAsync(Arg.Any<Stream>()).ReturnsForAnyArgs("dummyFileName.txt");
            var fileController = new FileController(TestLogger.Create<FileController>(_testOutputHelper), fileService);
            fileController.ControllerContext = await RequestWithFile(fileContents);

            await fileController.Upload();

            await fileService.Received(1).CreateFromStreamAsync(Arg.Any<Stream>());
        }

        private async Task<ControllerContext> RequestWithFile(string fileContents)
        {
            using (var formDataContent = new MultipartFormDataContent("---9908908098"))
            {
                formDataContent.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(fileContents)), "files", "dummy.txt");

                var httpContext = new DefaultHttpContext();
                httpContext.Request.Headers.Add("Content-Type", "multipart/form-data; boundary=---9908908098");

                httpContext.Request.Body = new MemoryStream(await formDataContent.ReadAsByteArrayAsync());
                var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
                return new ControllerContext(actionContext);
            }
        }
    }
}
