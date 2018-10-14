using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Reactive.Linq;

namespace DotNetForce
{
    public static class JObjectExtension
    {
        public static T UnFlatten<T>(this T source) where T : JObject, new()
        {
            if (source == null)
            {
                return source;
            }

            var result = new T();
            foreach (var prop in source.Properties())
            {
                var propName = prop.Name.Split(new[] { ':' }, 2);
                result[propName[0]] = prop?.Type == JTokenType.Object
                    ? prop.Value.ToObject<JObject>().UnFlatten(propName.Skip(1).FirstOrDefault())
                    : prop.Value;
            }
            return result;
        }

        public static T UnFlatten<T>(this T value, string name) where T : JObject, new()
        {
            if (value == null || string.IsNullOrEmpty(name))
            {
                return value;
            }

            var names = name.Split(new[] { ':' }, 2);
            if (names.Length > 1)
            {
                return new T { [names[0]] = value.UnFlatten(names[1]) };
            }

            return new T { [name] = value };
        }

        public static T Assign<T>(this T source, params JObject[] others) where T : JObject
        {
            if (source == null)
            {
                return source;
            }

            if (others?.Length > 0)
            {
                foreach (JObject other in others)
                {
                    foreach (var prop in other.Properties())
                    {
                        source[prop.Name] = prop.Value;
                    }
                }
            }
            return source;
        }

        public static T Pick<T>(this T source, params string[] colNames) where T : JObject, new()
        {
            if (source == null)
            {
                return source;
            }

            var result = new T();
            if ((colNames?.Length ?? 0) == 0)
            {
                foreach (var prop in source.Properties())
                {
                    result[prop.Name] = prop.Value;
                }
            }
            else
            {
                foreach (var prop in source.Properties())
                {
                    if (colNames.Contains(prop.Name))
                    {
                        result[prop.Name] = prop.Value;
                    }
                }
            }
            return result;
        }

        public static T Omit<T>(this T source, params string[] colNames) where T : JObject, new()
        {
            if (source == null)
            {
                return source;
            }

            var result = new T();
            if ((colNames?.Length ?? 0) == 0)
            {
                foreach (var prop in source.Properties())
                {
                    result[prop.Name] = prop.Value;
                }
            }
            else
            {
                foreach (var prop in source.Properties())
                {
                    if (!colNames.Contains(prop.Name))
                    {
                        result[prop.Name] = prop.Value;
                    }
                }
            }
            return result;
        }
    }
}
