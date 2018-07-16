using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bogus;
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

        [Fact]
        public async Task UploadFileTest()
        {
            var fileContent = "This is some dummy file contents";

            var fileService = Substitute.For<ITempFileService>();

            string receiveFileContent = null;

            fileService.CreateFromStreamAsync(Arg.Do<Stream>(stream =>
            {
                receiveFileContent = new StreamReader(stream, Encoding.UTF8).ReadToEnd();
            })).ReturnsForAnyArgs("dummyFileName.txt");

            var fileController = new FileController(TestLogger.Create<FileController>(_testOutputHelper), fileService)
            {
                ControllerContext = await RequestWithFiles(fileContent)
            };

            await fileController.Upload();

            await fileService.Received(1).CreateFromStreamAsync(Arg.Any<Stream>());

            receiveFileContent.Should().Be(fileContent);
        }

        [Fact]
        public async Task UploadFilesTest()
        {
            var fileContents = new Faker<string>().CustomInstantiator(f => f.Lorem.Paragraphs(3, 10)).Generate(4).ToArray();

            var fileService = Substitute.For<ITempFileService>();

            var receivedFileContents = new List<string>();

            fileService.CreateFromStreamAsync(Arg.Do<Stream>(stream =>
            {
                receivedFileContents.Add(new StreamReader(stream, Encoding.UTF8).ReadToEnd());
            })).ReturnsForAnyArgs("dummyFileName.txt");

            var fileController = new FileController(TestLogger.Create<FileController>(_testOutputHelper), fileService)
            {
                ControllerContext = await RequestWithFiles(fileContents)
            };

            await fileController.Upload();

            await fileService.Received(fileContents.Length).CreateFromStreamAsync(Arg.Any<Stream>());

            receivedFileContents.Should().BeEquivalentTo(fileContents);
        }

        private static async Task<ControllerContext> RequestWithFiles(params string[] fileContents)
        {
            var boundary = "---9908908098";

            using (var formDataContent = new MultipartFormDataContent(boundary))
            {
                for (var x = 0; x < fileContents.Length; x++)
                {
                    formDataContent.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(fileContents[x])), "files", $"dummy.{x}.txt");
                }
                
                var httpContext = new DefaultHttpContext();
                httpContext.Request.Headers.Add("Content-Type", $"multipart/form-data; boundary={boundary}");

                httpContext.Request.Body = new MemoryStream(await formDataContent.ReadAsByteArrayAsync());
                var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
                return new ControllerContext(actionContext);
            }
        }
    }
}
