using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MSBLOC.Core.Model;
using MSBLOC.Web.Controllers.api;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;
using MSBLOC.Web.Tests.Util;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Web.Tests.Controllers.api
{
    public class LogControllerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<LogControllerTests> _logger;
        private static readonly Faker Faker;

        public LogControllerTests(ITestOutputHelper testOutputHelper)
        {
            _logger = TestLogger.Create<LogControllerTests>(testOutputHelper);
            _testOutputHelper = testOutputHelper;
        }

        static LogControllerTests()
        {
            Faker = new Faker();
        }

        [Fact]
        public async Task UploadFileTest()
        {
            var name = "dummyFileName.txt";
            var fileContent = "This is some dummy file contents";

            var fileDictionary = new Dictionary<string, string> {{name, fileContent}};

            var fileService = Substitute.For<ITempFileService>();
            var msblocService = Substitute.For<IMSBLOCService>();

            var receivedFiles = new Dictionary<string, string>();

            fileService.CreateFromStreamAsync(Arg.Any<string>(), Arg.Any<Stream>())
                .Returns(ci =>
                {
                    var fileName = (string) ci[0];
                    var stream = (Stream) ci[1];
                    receivedFiles.Add(fileName, new StreamReader(stream, Encoding.UTF8).ReadToEnd());
                    return $"temp/{fileName}";
                });

            var fileController = new LogControllerStub(TestLogger.Create<LogController>(_testOutputHelper), fileService,
                msblocService)
            {
                ControllerContext = await RequestWithFiles(fileDictionary),
                MetadataProvider = new EmptyModelMetadataProvider(),
                ModelBinderFactory = Substitute.For<IModelBinderFactory>(),
                ObjectValidator = Substitute.For<IObjectModelValidator>()
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
            var msblocService = Substitute.For<IMSBLOCService>();

            var receivedFiles = new Dictionary<string, string>();

            fileService.CreateFromStreamAsync(Arg.Any<string>(), Arg.Any<Stream>())
                .Returns(ci =>
                {
                    var fileName = (string) ci[0];
                    var stream = (Stream) ci[1];
                    receivedFiles.Add(fileName, new StreamReader(stream, Encoding.UTF8).ReadToEnd());
                    return $"temp/{fileName}";
                });

            var fileController = new LogControllerStub(TestLogger.Create<LogController>(_testOutputHelper), fileService,
                msblocService)
            {
                ControllerContext = await RequestWithFiles(fileContents),
                MetadataProvider = new EmptyModelMetadataProvider(),
                ModelBinderFactory = Substitute.For<IModelBinderFactory>(),
                ObjectValidator = Substitute.For<IObjectModelValidator>()
            };

            await fileController.Upload();

            await fileService.Received(1).CreateFromStreamAsync(Arg.Any<string>(), Arg.Any<Stream>());

            receivedFiles.Count.Should().Be(1);
        }

        [Fact]
        public async Task UploadBadRequestTest()
        {
            var fileService = Substitute.For<ITempFileService>();
            var msblocService = Substitute.For<IMSBLOCService>();

            var fileController = new LogControllerStub(TestLogger.Create<LogController>(_testOutputHelper), fileService,
                msblocService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                },
                MetadataProvider = new EmptyModelMetadataProvider(),
                ModelBinderFactory = Substitute.For<IModelBinderFactory>(),
                ObjectValidator = Substitute.For<IObjectModelValidator>()
            };

            var result = await fileController.Upload() as BadRequestObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task UploadFileWithMissingBinaryLogFileName()
        {
            var name = "dummyFileName.txt";
            var fileContent = "This is some dummy file contents";

            var fileDictionary = new Dictionary<string, string> { { name, fileContent } };

            var fileService = Substitute.For<ITempFileService>();
            var msblocService = Substitute.For<IMSBLOCService>();

            var receivedFiles = new Dictionary<string, string>();

            fileService.CreateFromStreamAsync(Arg.Any<string>(), Arg.Any<Stream>())
                .Returns(ci =>
                {
                    var fileName = (string)ci[0];
                    var stream = (Stream)ci[1];
                    receivedFiles.Add(fileName, new StreamReader(stream, Encoding.UTF8).ReadToEnd());
                    return $"temp/{fileName}";
                });
            fileService.Files.Returns(new[] { name });

            var formData = new SubmissionData
            {
                RepoName = "SomeApplicationName",
                RepoOwner = "SomeApplicationOwner",
                CommitSha = "12345",
                CloneRoot = "c:/cloneRoot",
                BinaryLogFile = string.Empty //Bad Data
            };

            var fileController = new LogControllerStub(TestLogger.Create<LogController>(_testOutputHelper), fileService,
                msblocService)
            {
                ControllerContext = await RequestWithFiles(fileDictionary, formData),
                MetadataProvider = new EmptyModelMetadataProvider(),
                ModelBinderFactory = Substitute.For<IModelBinderFactory>(),
                ObjectValidator = Substitute.For<IObjectModelValidator>()
            };

            var result = await fileController.Upload() as BadRequestObjectResult;
            result.Should().NotBeNull();
            result.Value.Should().BeOfType<SerializableError>();
        }

        [Fact]
        public async Task UploadWithMissingFile()
        {
            var name = "dummyFileName.txt";
            var fileContent = "This is some dummy file contents";

            var fileDictionary = new Dictionary<string, string> { { name, fileContent } };

            var fileService = Substitute.For<ITempFileService>();
            var msblocService = Substitute.For<IMSBLOCService>();

            var receivedFiles = new Dictionary<string, string>();

            fileService.CreateFromStreamAsync(Arg.Any<string>(), Arg.Any<Stream>())
                .Returns(ci =>
                {
                    var fileName = (string)ci[0];
                    var stream = (Stream)ci[1];
                    receivedFiles.Add(fileName, new StreamReader(stream, Encoding.UTF8).ReadToEnd());
                    return $"temp/{fileName}";
                });
            fileService.Files.Returns(new[] { name });

            var formData = new SubmissionData
            {
                RepoName = "SomeApplicationName",
                RepoOwner = "SomeApplicationOwner",
                CommitSha = "12345",
                CloneRoot = "c:/cloneRoot",
                BinaryLogFile = "someOtherFileName.txt" //Bad Data
            };

            var fileController = new LogControllerStub(TestLogger.Create<LogController>(_testOutputHelper), fileService,
                msblocService)
            {
                ControllerContext = await RequestWithFiles(fileDictionary, formData),
                MetadataProvider = new EmptyModelMetadataProvider(),
                ModelBinderFactory = Substitute.For<IModelBinderFactory>(),
                ObjectValidator = Substitute.For<IObjectModelValidator>()
            };

            var result = await fileController.Upload() as BadRequestObjectResult;
            result.Should().NotBeNull();
            result.Value.Should().BeOfType<SerializableError>();
        }

        [Fact]
        public async Task UploadFileWithFormData()
        {
            var name = "dummyFileName.txt";
            var fileContent = "This is some dummy file contents";

            var fileDictionary = new Dictionary<string, string> {{name, fileContent}};

            var fileService = Substitute.For<ITempFileService>();
            var msblocService = Substitute.For<IMSBLOCService>();

            var receivedFiles = new Dictionary<string, string>();

            fileService.CreateFromStreamAsync(Arg.Any<string>(), Arg.Any<Stream>())
                .Returns(ci =>
                {
                    var fileName = (string) ci[0];
                    var stream = (Stream) ci[1];
                    receivedFiles.Add(fileName, new StreamReader(stream, Encoding.UTF8).ReadToEnd());
                    return $"temp/{fileName}";
                });
            fileService.Files.Returns(new[] {name});

            var checkRun = new CheckRun()
            {
                Id = Faker.Random.Long(),
                Url = Faker.Internet.Url()
            };

            msblocService.SubmitAsync(null).ReturnsForAnyArgs(checkRun);

            var formData = new SubmissionData
            {
                RepoName = "SomeApplicationName",
                RepoOwner = "SomeApplicationOwner",
                CommitSha = "12345",
                CloneRoot = "c:/cloneRoot"
            };

            var fileController = new LogControllerStub(TestLogger.Create<LogController>(_testOutputHelper), fileService,
                msblocService)
            {
                ControllerContext = await RequestWithFiles(fileDictionary, formData),
                MetadataProvider = new EmptyModelMetadataProvider(),
                ModelBinderFactory = Substitute.For<IModelBinderFactory>(),
                ObjectValidator = Substitute.For<IObjectModelValidator>()
            };

            var result = await fileController.Upload() as JsonResult;

            await fileService.Received(1).CreateFromStreamAsync(Arg.Is(name), Arg.Any<Stream>());
            await msblocService.Received(1).SubmitAsync(Arg.Is<SubmissionData>(data =>
                data.RepoOwner.Equals(formData.RepoOwner) &&
                data.RepoName.Equals(formData.RepoName) && 
                data.CloneRoot.Equals(formData.CloneRoot) &&
                data.CommitSha.Equals(formData.CommitSha) &&
                data.BinaryLogFile.Equals(receivedFiles.Keys.FirstOrDefault())));

            receivedFiles.Should().BeEquivalentTo(fileDictionary);

            var resultFormData = result?.Value as CheckRun;

            resultFormData.Should().NotBeNull();
            resultFormData.Id.Should().Be(checkRun.Id);
            resultFormData.Url.Should().Be(checkRun.Url);
        }

        private static async Task<ControllerContext> RequestWithFiles(IDictionary<string, string> fileDictionary,
            SubmissionData formData = null)
        {
            var boundary = "---9908908098";

            var isFirst = true;

            using (var formDataContent = new MultipartFormDataContent(boundary))
            {
                foreach (var kvp in fileDictionary)
                {
                    var fileRole = (isFirst) ? nameof(SubmissionData.BinaryLogFile) : "SomeOtherUnusedRole";
                    isFirst = false;
                    formDataContent.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(kvp.Value)), fileRole, kvp.Key);
                }

                if (formData != null)
                {
                    var formDataDictionary =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(
                            JsonConvert.SerializeObject(formData));

                    foreach (var kvp in formDataDictionary.Where(i => i.Value != null))
                    {
                        formDataContent.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(kvp.Value)), kvp.Key);
                    }
                }

                var httpContext = new DefaultHttpContext();
                httpContext.Request.Headers.Add("Content-Type", $"multipart/form-data; boundary={boundary}");

                httpContext.Request.Body = new MemoryStream(await formDataContent.ReadAsByteArrayAsync());
                var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
                return new ControllerContext(actionContext);
            }
        }

        private class LogControllerStub : LogController
        {
            public LogControllerStub(ILogger<LogController> logger, ITempFileService tempFileService, IMSBLOCService msblocService) : base(logger, tempFileService, msblocService)
            {
            }

            protected override Task<bool> BindDataAsync(SubmissionData model, Dictionary<string, StringValues> dataToBind)
            {
                foreach (var item in dataToBind)
                {
                    var propertyInfo = model.GetType().GetProperty(item.Key);
                    var value = Convert.ChangeType(item.Value.ToString(), propertyInfo.PropertyType);
                    propertyInfo.SetValue(model, value);
                }

                return Task.FromResult(true);
            }
        }
    }
}
