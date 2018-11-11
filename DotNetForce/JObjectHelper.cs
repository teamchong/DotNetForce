using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Reactive.Linq;

namespace DotNetForce
{
    internal class JObjectHelper<T> where T : JObject, new()
    {
        public T Source { get; set; }
        
        internal JObjectHelper(T src)
        {
            Source = src;
        }

        public T UnFlatten() 
        {
            if (Source == null)
            {
                return Source;
            }

            var result = new T();
            foreach (var prop in Source.Properties())
            {
                var propName = prop.Name.Split(new[] { ':' }, 2);
                result[propName[0]] = prop?.Type == JTokenType.Object
                    ? new JObjectHelper<T>((T)prop.Value).UnFlatten(propName.Skip(1).FirstOrDefault())
                    : prop.Value;
            }
            return result;
        }

        public T UnFlatten(string name)
        {
            if (Source == null || string.IsNullOrEmpty(name))
            {
                return Source;
            }

            var names = name.Split(new[] { ':' }, 2);
            if (names.Length > 1)
            {
                return new T { [names[0]] = UnFlatten(names[1]) };
            }

            return new T { [name] = Source };
        }

        public T Assign(params JObject[] others)
        {
            if (Source == null)
            {
                return Source;
            }

            if (others?.Length > 0)
            {
                foreach (JObject other in others)
                {
                    foreach (var prop in other.Properties())
                    {
                        Source[prop.Name] = prop.Value;
                    }
                }
            }
            return Source;
        }

        public T Pick(params string[] colNames)
        {
            if (Source == null)
            {
                return Source;
            }

            var result = new T();
            if ((colNames?.Length ?? 0) == 0)
            {
                foreach (var prop in Source.Properties())
                {
                    result[prop.Name] = prop.Value;
                }
            }
            else
            {
                foreach (var prop in Source.Properties())
                {
                    if (colNames.Contains(prop.Name))
                    {
                        result[prop.Name] = prop.Value;
                    }
                }
            }
            return result;
        }

        public T Omit(params string[] colNames)
        {
            if (Source == null)
            {
                return Source;
            }

            var result = new T();
            if ((colNames?.Length ?? 0) == 0)
            {
                foreach (var prop in Source.Properties())
                {
                    result[prop.Name] = prop.Value;
                }
            }
            else
            {
                foreach (var prop in Source.Properties())
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
