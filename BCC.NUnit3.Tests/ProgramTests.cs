using Bogus;
using Xunit;

namespace BCC.NUnit3.Tests
{
    public class ProgramTests
    {
        private static readonly Faker Faker;

        static ProgramTests()
        {
            Faker = new Faker();
        }

        [Fact]
        public void ShouldRun()
        {
            var program = new Program();

            program.Run(new string[0]);
        }
    }
}
