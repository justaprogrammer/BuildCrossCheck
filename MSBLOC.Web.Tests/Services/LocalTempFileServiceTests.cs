using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Services;
using MSBLOC.Web.Services;
using MSBLOC.Web.Tests.Util;
using NSubstitute.ExceptionExtensions;
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
        [MemberData(nameof(FilesWithMd5))]
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

        [Fact]
        public async Task ReplaceFileTest()
        {
            var resourceFileName = (string) FilesWithMd5[0][0];
            var path = GetResourcePath(resourceFileName);
            var replacementPath = GetResourcePath((string)FilesWithMd5[1][0]);

            File.Exists(path).Should().BeTrue();
            File.Exists(replacementPath).Should().BeTrue();

            string tempFilePath1;
            string tempFilePath2;

            using (var fileService = new LocalTempFileService(TestLogger.Create<LocalTempFileService>(_testOutputHelper)))
            {
                tempFilePath1 = await fileService.CreateFromStreamAsync(resourceFileName, File.OpenRead(path));

                tempFilePath1.Should().NotBeNullOrWhiteSpace();
                var tempMd5 = ComputeFileMd5(tempFilePath1);
                tempMd5.Should().Be((string) FilesWithMd5[0][1]);
                fileService.Files.Should().BeEquivalentTo(new[] { resourceFileName });
                fileService.GetFilePath(resourceFileName).Should().Be(tempFilePath1);

                tempFilePath2 = await fileService.CreateFromStreamAsync(resourceFileName, File.OpenRead(replacementPath));

                File.Exists(tempFilePath1).Should().BeFalse();

                tempFilePath2.Should().NotBeNullOrWhiteSpace();
                tempFilePath2.Should().NotBe(tempFilePath1);
                tempMd5 = ComputeFileMd5(tempFilePath2);
                tempMd5.Should().Be((string) FilesWithMd5[1][1]);
                fileService.Files.Should().BeEquivalentTo(new[] { resourceFileName });
                fileService.GetFilePath(resourceFileName).Should().Be(tempFilePath2);
            }

            File.Exists(tempFilePath1).Should().BeFalse();
            File.Exists(tempFilePath2).Should().BeFalse();
        }

        [Fact]
        public async Task GetFilePathTest()
        {
            var resourceFileNames = FilesWithMd5.Select(i => (string) i[0]).ToList();

            var tempFilePaths = new List<string>();

            using (var fileService = new LocalTempFileService(TestLogger.Create<LocalTempFileService>(_testOutputHelper)))
            {
                foreach (var resourceFileName in resourceFileNames)
                {
                    var path = GetResourcePath(resourceFileName);
                    File.Exists(path).Should().BeTrue();

                    var tempFilePath = await fileService.CreateFromStreamAsync(resourceFileName, File.OpenRead(path));
                    tempFilePath.Should().NotBeNullOrWhiteSpace();
                    tempFilePaths.Add(tempFilePath);
                }
                
                fileService.Files.Should().BeEquivalentTo(resourceFileNames);
            }

            foreach (var tempFilePath in tempFilePaths)
            {
                File.Exists(tempFilePath).Should().BeFalse();
            }
        }

        [Fact]
        public void GetUnknownFileTest()
        {
            using (var fileService = new LocalTempFileService(TestLogger.Create<LocalTempFileService>(_testOutputHelper)))
            {
                fileService.Invoking(f => f.GetFilePath("someFile.txt"))
                    .Should().Throw<FileNotFoundException>();
            }
        }

        public static IList<object[]> FilesWithMd5 => new List<object[]>
        {
            new object[] {"testconsoleapp1-1error.binlog", "137b21fc04092a1ee68b32b981707ce4"},
            new object[] {"testconsoleapp1-1warning.binlog", "85a075932d54c3d5dfb5c6ba99c3485d"}
        };

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
