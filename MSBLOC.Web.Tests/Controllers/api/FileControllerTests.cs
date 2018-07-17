using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var name = "dummyFileName.txt";
            var fileContent = "This is some dummy file contents";

            var fileDictionary = new Dictionary<string, string>{{name, fileContent}};

            var fileService = Substitute.For<ITempFileService>();

            var receivedFiles = new Dictionary<string, string>();

            fileService.CreateFromStreamAsync(Arg.Any<string>(), Arg.Any<Stream>())
                .Returns(ci =>
                {
                    var fileName = (string)ci[0];
                    var stream = (Stream)ci[1];
                    receivedFiles.Add(fileName, new StreamReader(stream, Encoding.UTF8).ReadToEnd());
                    return $"temp/{ci[0]}";
                });

            var fileController = new FileController(TestLogger.Create<FileController>(_testOutputHelper), fileService)
            {
                ControllerContext = await RequestWithFiles(fileDictionary)
            };

            await fileController.Upload();

            await fileService.Received(1).CreateFromStreamAsync(Arg.Is(name), Arg.Any<Stream>());

            receivedFiles.Should().BeEquivalentTo(fileDictionary);
        }

        [Fact]
        public async Task UploadFilesTest()
        {
            var fileContents = new Faker<string>().CustomInstantiator(f => f.Lorem.Paragraphs(3, 10)).Generate(4)
                .ToDictionary(f => $"{string.Join("_", new Faker().Lorem.Words(4))}.txt", f => f);

            var fileService = Substitute.For<ITempFileService>();

            var receivedFiles = new Dictionary<string, string>();

            fileService.CreateFromStreamAsync(Arg.Any<string>(), Arg.Any<Stream>())
                .Returns(ci =>
                {
                    var fileName = (string) ci[0];
                    var stream = (Stream) ci[1];
                    receivedFiles.Add(fileName ,new StreamReader(stream, Encoding.UTF8).ReadToEnd());
                    return $"temp/{ci[0]}";
                });

            var fileController = new FileController(TestLogger.Create<FileController>(_testOutputHelper), fileService)
            {
                ControllerContext = await RequestWithFiles(fileContents)
            };

            await fileController.Upload();

            await fileService.Received(fileContents.Count).CreateFromStreamAsync(Arg.Any<string>(), Arg.Any<Stream>());

            receivedFiles.Should().BeEquivalentTo(fileContents);
        }

        private static async Task<ControllerContext> RequestWithFiles(IDictionary<string, string> fileDictionary)
        {
            var boundary = "---9908908098";

            using (var formDataContent = new MultipartFormDataContent(boundary))
            {
                foreach (var kvp in fileDictionary)
                {
                    formDataContent.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(kvp.Value)), "files", kvp.Key);
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
