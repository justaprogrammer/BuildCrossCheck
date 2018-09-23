namespace BCC.Core.Tests.Util
{
    public static class TestLogger
    {
        public static ILogger<T> Create<T>(ITestOutputHelper output)
        {
            var logger = new XUnitLogger<T>(output);
            return logger;
        }

        class XUnitLogger<T> : ILogger<T>, IDisposable
        {
            private readonly Action<string> _output;

            public XUnitLogger(ITestOutputHelper testOutputHelper)
            {
                _output = testOutputHelper.WriteLine;
            }

            public void Dispose()
            {
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter) => _output(formatter(state, exception));

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => this;
        }
    }
}