namespace BCC.Web.Util
{
    public class InMemoryCachingInterceptor : IInterceptor
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        public void Intercept(IInvocation invocation)
        {
            var cacheKey = GenerateCacheKey(invocation.Method.Name, invocation.Arguments);

            if (!_cache.ContainsKey(cacheKey))
            {
                lock (_cache)
                {
                    if (!_cache.ContainsKey(cacheKey))
                    {
                        invocation.Proceed();
                        _cache[cacheKey] = invocation.ReturnValue;
                    }
                }
            }

            invocation.ReturnValue = _cache[cacheKey];
        }

        private static string GenerateCacheKey(string methodName, IReadOnlyCollection<object> arguments)
        {
            if (arguments == null || arguments.Count == 0) return methodName;

            return $"{methodName}::{string.Join("::", arguments.Select(arg => arg.ToString()))}";
        }
    }
}
