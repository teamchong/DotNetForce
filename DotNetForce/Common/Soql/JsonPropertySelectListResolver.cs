using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace DotNetForce.Common.Soql
{
    [JetBrains.Annotations.PublicAPI]
    public class JsonPropertySelectListResolver : ISelectListResolver
    {
        private readonly bool _ignorePropsWithoutAttribute;

        public JsonPropertySelectListResolver(bool ignorePropsWithoutAttribute = false)
        {
            _ignorePropsWithoutAttribute = ignorePropsWithoutAttribute;
        }

        public string GetFieldsList<T>()
        {
            var propInfo = typeof(T).GetRuntimeProperties();

            if (_ignorePropsWithoutAttribute)
                propInfo = propInfo.Where(p => p.GetCustomAttribute<JsonPropertyAttribute>() != null);

            var fields = propInfo.Select(p =>
            {
                var customAttribute = p.GetCustomAttribute<JsonPropertyAttribute>();
                return customAttribute?.PropertyName ?? p.Name;
            });

            return string.Join(", ", fields);
        }
    }
}
