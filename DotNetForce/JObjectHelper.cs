using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace DotNetForce
{
    internal class JObjectHelper<T> where T : JToken
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

            var result = JToken.FromObject(new Dictionary<string, JToken>());
            foreach (var prop in (IDictionary<string, JToken>)Source)
            {
                var propNames = prop.Key.Split(new[] { ':' }, 2);
                result[propNames[0]] = prop.Value?.Type == JTokenType.Object
                    ? new JObjectHelper<T>((T)prop.Value).UnFlatten(propNames.Skip(1).FirstOrDefault())
                    : prop.Value;
            }
            return (T)result;
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
                return (T)JToken.FromObject(new Dictionary<string, JToken> { [names[0]] = UnFlatten(names[1]) });
            }

            return (T)JToken.FromObject(new Dictionary<string, JToken> { [name] = Source });
        }

        public T Assign(params JToken[] others)
        {
            if (Source == null)
            {
                return Source;
            }

            if (others?.Length > 0)
            {
                foreach (JToken other in others)
                {
                    foreach (var prop in ((IDictionary<string, JToken>)other))
                    {
                        Source[prop.Key] = prop.Value;
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

            var result = JToken.FromObject(new Dictionary<string, JToken>());
            if ((colNames?.Length ?? 0) == 0)
            {
                foreach (var prop in (IDictionary<string, JToken>)Source)
                {
                    result[prop.Key] = prop.Value;
                }
            }
            else
            {
                foreach (var prop in (IDictionary<string, JToken>)Source)
                {
                    if (colNames.Contains(prop.Key))
                    {
                        result[prop.Key] = prop.Value;
                    }
                }
            }
            return (T)result;
        }

        public T Omit(params string[] colNames)
        {
            if (Source == null)
            {
                return Source;
            }

            var result = JToken.FromObject(new Dictionary<string, JToken>());
            if ((colNames?.Length ?? 0) == 0)
            {
                foreach (var prop in (IDictionary<string, JToken>)Source)
                {
                    result[prop.Key] = prop.Value;
                }
            }
            else
            {
                foreach (var prop in (IDictionary<string, JToken>)Source)
                {
                    if (!colNames.Contains(prop.Key))
                    {
                        result[prop.Key] = prop.Value;
                    }
                }
            }
            return (T)result;
        }
    }
}
