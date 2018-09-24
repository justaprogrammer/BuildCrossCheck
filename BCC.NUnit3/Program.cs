using System.Diagnostics.CodeAnalysis;

namespace BCC.NUnit3
{
    public class Program
    {
        [ExcludeFromCodeCoverage]
        static int Main(string[] args)
        {
            var program = new Program();
            return program.Run(args) ? 0 : 1;
        }

        public Program()
        {
            //TestResult testResult;
        }

        public bool Run(string[] args)
        {
            return true;
        }
    }
}
