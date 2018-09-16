using FluentAssertions;
using MSBLOC.MSBuildLog.Console.Extensions;
using Xunit;

namespace MSBLOC.MSBuildLog.Console.Tests.Extensions
{
    // https://stackoverflow.com/a/31941159/104877
    public class IsSubPathOfStringExtensionTest
    {
        [Theory]
        [InlineData(@"c:\foo", @"c:",true)]
        [InlineData(@"c:\foo", @"c:\",true)]
        [InlineData(@"c:\foo", @"c:\foo",true)]
        [InlineData(@"c:\foo", @"c:\foo\",true)]
        [InlineData(@"c:\foo\", @"c:\foo",true)]
        [InlineData(@"c:\foo\bar\", @"c:\foo\",true)]
        [InlineData(@"c:\foo\bar", @"c:\foo\",true)]
        [InlineData(@"c:\foo\a.txt", @"c:\foo",true)]
        [InlineData(@"c:\FOO\a.txt", @"c:\foo",true)]
        [InlineData(@"c:/foo/a.txt", @"c:\foo",true)]
        [InlineData(@"c:\foobar", @"c:\foo",false)]
        [InlineData(@"c:\foobar\a.txt", @"c:\foo",false)]
        [InlineData(@"c:\foobar\a.txt", @"c:\foo\",false)]
        [InlineData(@"c:\foo\a.txt", @"c:\foobar",false)]
        [InlineData(@"c:\foo\a.txt", @"c:\foobar\",false)]
        [InlineData(@"c:\foo\..\bar\baz", @"c:\foo",false)]
        [InlineData(@"c:\foo\..\bar\baz", @"c:\bar",true)]
        [InlineData(@"c:\foo\..\bar\baz", @"c:\barr",false)]
        public void IsSubPathOfTest(string path, string baseDirPath, bool expected)
        {
            path.IsSubPathOf(baseDirPath).Should().Be(expected);
        }
    }
}
