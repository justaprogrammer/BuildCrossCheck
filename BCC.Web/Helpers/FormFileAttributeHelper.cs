using BCC.Web.Attributes;

namespace BCC.Web.Helpers
{
    public static class FormFileAttributeHelper
    {
        private static readonly ConcurrentDictionary<Tuple<Type, bool>, IList<PropertyInfo>> FormFileNamesDictionary
            = new ConcurrentDictionary<Tuple<Type, bool>, IList<PropertyInfo>>();

        public static IList<PropertyInfo> GetFormFilePropertyInfos(Type t, bool required = false)
        {
            return FormFileNamesDictionary.GetOrAdd(new Tuple<Type, bool>(t, required),
                tuple => tuple.Item1.GetProperties()
                    .Where(p => p.GetCustomAttributes(typeof(FormFileAttribute), false).Any())
                    .Where(p => !tuple.Item2 || p.GetCustomAttributes(typeof(RequiredAttribute), true).Any())
                    .ToList());
        }

        public static IList<string> GetFormFileNames(Type t) =>
            GetFormFilePropertyInfos(t)
                .Select(info => info.Name)
                .ToList();

        public static IList<PropertyInfo> GetRequiredFormFileProperties(Type t) => GetFormFilePropertyInfos(t, true);
    }
}