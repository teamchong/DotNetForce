using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace DotNetForce.Common.Soql
{
    public class DataMemberSelectListResolver : ISelectListResolver
    {
        public string GetFieldsList<T>()
        {
            var fields = typeof(T).GetRuntimeProperties()
                .Where(p =>
                {
                    var customAttribute = p.GetCustomAttribute<IgnoreDataMemberAttribute>();
                    return customAttribute == null;
                })
                .Select(p =>
                {
                    var customAttribute = p.GetCustomAttribute<DataMemberAttribute>();
                    return customAttribute?.Name ?? p.Name;
                });

            return string.Join(", ", fields);
        }
    }
}
