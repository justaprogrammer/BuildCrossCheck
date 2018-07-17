using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Web.Services;
using MSBLOC.Web.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Web.Tests.Services
{
    public class LocalTempFileServiceTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<LocalTempFileServiceTests> _logger;

        public LocalTempFileServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<LocalTempFileServiceTests>(testOutputHelper);
        }

        private static string GetResourcePath(string file)
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            dirPath.Should().NotBeNull();
            return Path.Combine(dirPath, "Resources", file);
        }

        [Theory]
        [InlineData("testconsoleapp1-1error.binlog", "137b21fc04092a1ee68b32b981707ce4")]
        public async Task CreateFromStreamAsyncTest(string resourceFileName, string md5)
        {
            var path = GetResourcePath(resourceFileName);
            File.Exists(path).Should().BeTrue();

            string tempFilePath;

            using (var fileService = new LocalTempFileService(TestLogger.Create<LocalTempFileService>(_testOutputHelper)))
            {
                tempFilePath = await fileService.CreateFromStreamAsync(resourceFileName, File.OpenRead(path));
                tempFilePath.Should().NotBeNullOrWhiteSpace();

                var tempMd5 = ComputeFileMd5(tempFilePath);

                tempMd5.Should().Be(md5);
            }

            File.Exists(tempFilePath).Should().BeFalse();
        }

        private static string ComputeFileMd5(string fileName)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    var bytes = md5.ComputeHash(stream);

                    var result = new StringBuilder(bytes.Length * 2);

                    foreach (var i in bytes)
                    {
                        result.Append(i.ToString("x2"));
                    }

                    return result.ToString();
                }
            }
        }
    }
}
