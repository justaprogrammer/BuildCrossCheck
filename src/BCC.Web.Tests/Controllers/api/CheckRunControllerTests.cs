using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCC.Web.Controllers.Api;
using BCC.Web.Interfaces;
using BCC.Web.Models;
using BCC.Web.Models.GitHub;
using BCC.Web.Services;
using BCC.Web.Tests.Util;
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
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace BCC.Web.Tests.Controllers.api
{
    public class CheckRunControllerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<CheckRunControllerTests> _logger;
        private static readonly Faker Faker;

        public CheckRunControllerTests(ITestOutputHelper testOutputHelper)
        {
            _logger = TestLogger.Create<CheckRunControllerTests>(testOutputHelper);
            _testOutputHelper = testOutputHelper;
        }

        static CheckRunControllerTests()
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
            var checkRunSubmissionService = Substitute.For<Web.Interfaces.ICheckRunSubmissionService>();

            var receivedFiles = new Dictionary<string, string>();

            fileService.CreateFromStreamAsync(Arg.Any<string>(), Arg.Any<Stream>())
                .Returns(ci =>
                {
                    var fileName = (string) ci[0];
                    var stream = (Stream) ci[1];
                    receivedFiles.Add(fileName, new StreamReader(stream, Encoding.UTF8).ReadToEnd());
                    return $"temp/{fileName}";
                });

            var checkRunController = new CheckRunControllerStub(TestLogger.Create<CheckRunController>(_testOutputHelper), fileService, checkRunSubmissionService, Substitute.For<ITelemetryService>())
            {
                ControllerContext = await RequestWithFiles(fileDictionary),
                MetadataProvider = new EmptyModelMetadataProvider(),
                ModelBinderFactory = Substitute.For<IModelBinderFactory>(),
                ObjectValidator = Substitute.For<IObjectModelValidator>()
            };

            await checkRunController.Upload();

            await fileService.Received(1).CreateFromStreamAsync(Arg.Is(name), Arg.Any<Stream>());

            receivedFiles.Should().BeEquivalentTo(fileDictionary);
        }

        [Fact]
        public async Task UploadFilesTest()
        {
            var fileContents = new Faker<string>().CustomInstantiator(f => f.Lorem.Paragraphs(3, 10)).Generate(4)
                .ToDictionary(f => $"{string.Join("_", new Faker().Lorem.Words(4))}.txt", f => f);

            var fileService = Substitute.For<ITempFileService>();
            var checkRunSubmissionService = Substitute.For<Web.Interfaces.ICheckRunSubmissionService>();

            var receivedFiles = new Dictionary<string, string>();

            fileService.CreateFromStreamAsync(Arg.Any<string>(), Arg.Any<Stream>())
                .Returns(ci =>
                {
                    var fileName = (string) ci[0];
                    var stream = (Stream) ci[1];
                    receivedFiles.Add(fileName, new StreamReader(stream, Encoding.UTF8).ReadToEnd());
                    return $"temp/{fileName}";
                });

            var checkRunController = new CheckRunControllerStub(TestLogger.Create<CheckRunController>(_testOutputHelper), fileService, checkRunSubmissionService, Substitute.For<ITelemetryService>())
            {
                ControllerContext = await RequestWithFiles(fileContents),
                MetadataProvider = new EmptyModelMetadataProvider(),
                ModelBinderFactory = Substitute.For<IModelBinderFactory>(),
                ObjectValidator = Substitute.For<IObjectModelValidator>()
            };

            await checkRunController.Upload();

            await fileService.Received(1).CreateFromStreamAsync(Arg.Any<string>(), Arg.Any<Stream>());

            receivedFiles.Count.Should().Be(1);
        }

        [Fact]
        public async Task UploadBadRequestTest()
        {
            var fileService = Substitute.For<ITempFileService>();
            var checkRunSubmissionService = Substitute.For<Web.Interfaces.ICheckRunSubmissionService>();

            var checkRunController = new CheckRunControllerStub(TestLogger.Create<CheckRunController>(_testOutputHelper), fileService, checkRunSubmissionService, Substitute.For<ITelemetryService>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                },
                MetadataProvider = new EmptyModelMetadataProvider(),
                ModelBinderFactory = Substitute.For<IModelBinderFactory>(),
                ObjectValidator = Substitute.For<IObjectModelValidator>()
            };

            var result = await checkRunController.Upload() as BadRequestObjectResult;

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
            var checkRunSubmissionService = Substitute.For<Web.Interfaces.ICheckRunSubmissionService>();

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

            var logUploadData = new LogUploadData
            {
                CommitSha = "12345",
                LogFile = string.Empty //Bad Data
            };

            var checkRunController = new CheckRunControllerStub(TestLogger.Create<CheckRunController>(_testOutputHelper), fileService, checkRunSubmissionService, Substitute.For<ITelemetryService>())
            {
                ControllerContext = await RequestWithFiles(fileDictionary, logUploadData),
                MetadataProvider = new EmptyModelMetadataProvider(),
                ModelBinderFactory = Substitute.For<IModelBinderFactory>(),
                ObjectValidator = Substitute.For<IObjectModelValidator>()
            };

            var result = await checkRunController.Upload() as BadRequestObjectResult;
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
            var checkRunSubmissionService = Substitute.For<Web.Interfaces.ICheckRunSubmissionService>();

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

            var logUploadData = new LogUploadData
            {
                CommitSha = "12345",
                LogFile = "someOtherFileName.txt" //Bad Data
            };

            var checkRunController = new CheckRunControllerStub(TestLogger.Create<CheckRunController>(_testOutputHelper), fileService, checkRunSubmissionService, Substitute.For<ITelemetryService>())
            {
                ControllerContext = await RequestWithFiles(fileDictionary, logUploadData),
                MetadataProvider = new EmptyModelMetadataProvider(),
                ModelBinderFactory = Substitute.For<IModelBinderFactory>(),
                ObjectValidator = Substitute.For<IObjectModelValidator>()
            };

            var result = await checkRunController.Upload() as BadRequestObjectResult;
            result.Should().NotBeNull();
            result.Value.Should().BeOfType<SerializableError>();
        }

        [Fact()]
        public async Task UploadFileWithFormData()
        {
            var name = "dummyFileName.txt";
            var fileContent = "This is some dummy file contents";

            var fileDictionary = new Dictionary<string, string> {{name, fileContent}};

            var fileService = Substitute.For<ITempFileService>();
            var checkRunSubmissionService = Substitute.For<Web.Interfaces.ICheckRunSubmissionService>();

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

            var checkRun = new CheckRun
            {
                Id = Faker.Random.Long(),
                Url = Faker.Internet.Url()
            };

            checkRunSubmissionService.SubmitAsync(null, null, null, null, 0).ReturnsForAnyArgs(checkRun);

            var logUploadData = new LogUploadData
            {
                CommitSha = "12345",
                PullRequestNumber = 345
            };

            var faker = new Faker();

            var repoOwner = faker.Person.FullName;
            var repoName = faker.Hacker.Phrase();

            var claims = new[]
            {
                new Claim("urn:bcc:repositoryName", repoName),
                new Claim("urn:bcc:repositoryOwner", repoOwner),
                new Claim("urn:bcc:repositoryOwnerId", faker.Random.Long().ToString())
            };

            var checkRunController = new CheckRunControllerStub(TestLogger.Create<CheckRunController>(_testOutputHelper), fileService, checkRunSubmissionService, Substitute.For<ITelemetryService>())
            {
                ControllerContext = await RequestWithFiles(fileDictionary, logUploadData, claims),
                MetadataProvider = new EmptyModelMetadataProvider(),
                ModelBinderFactory = Substitute.For<IModelBinderFactory>(),
                ObjectValidator = Substitute.For<IObjectModelValidator>()
            };

            var result = await checkRunController.Upload() as JsonResult;

            await fileService.Received(1).CreateFromStreamAsync(Arg.Is(name), Arg.Any<Stream>());
            await checkRunSubmissionService.Received(1).SubmitAsync(
                repoOwner,
                repoName,
                logUploadData.CommitSha,
                string.Empty,
                logUploadData.PullRequestNumber);

            receivedFiles.Should().BeEquivalentTo(fileDictionary);

            var resultFormData = result?.Value as CheckRun;

            resultFormData.Should().NotBeNull();
            resultFormData.Id.Should().Be(checkRun.Id);
            resultFormData.Url.Should().Be(checkRun.Url);
        }

        private static async Task<ControllerContext> RequestWithFiles(IDictionary<string, string> fileDictionary,
            LogUploadData formData = null,
            IEnumerable<Claim> claims = null)
        {
            var boundary = "---9908908098";

            var isFirst = true;

            using (var formDataContent = new MultipartFormDataContent(boundary))
            {
                foreach (var kvp in fileDictionary)
                {
                    var fileRole = (isFirst) ? nameof(LogUploadData.LogFile) : "SomeOtherUnusedRole";
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

                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims ?? Enumerable.Empty<Claim>()));

                httpContext.Request.Headers.Add("Content-Type", $"multipart/form-data; boundary={boundary}");

                httpContext.Request.Body = new MemoryStream(await formDataContent.ReadAsByteArrayAsync());
                var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
                return new ControllerContext(actionContext);
            }
        }

        private class CheckRunControllerStub : CheckRunController
        {
            public CheckRunControllerStub(ILogger<CheckRunController> logger, ITempFileService tempFileService,
                Web.Interfaces.ICheckRunSubmissionService checkRunSubmissionService, ITelemetryService telemetryService) : base(logger, tempFileService, checkRunSubmissionService, telemetryService)
            {

            }

            protected override Task<bool> BindModelAsync<T>(T model, Dictionary<string, StringValues> dataToBind)
            {
                foreach (var item in dataToBind)
                {
                    var propertyInfo = typeof(LogUploadData).GetProperty(item.Key);
                    var value = Convert.ChangeType(item.Value.ToString(), propertyInfo.PropertyType);
                    propertyInfo.SetValue(model, value);
                }

                return Task.FromResult(true);
            }
        }
    }
}
