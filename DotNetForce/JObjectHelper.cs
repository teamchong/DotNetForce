using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace DotNetForce
{
    internal class JObjectHelper
    {
        public JObject Source { get; set; }
        
        internal JObjectHelper(JObject src)
        {
            Source = src;
        }

        public JObject UnFlatten() 
        {
            if (Source == null)
            {
                return Source;
            }

            var result = new JObject();
            foreach (var prop in Source.Properties())
            {
                var propNames = prop.Name.Split(new[] { ':' }, 2);
                result[propNames[0]] = prop.Value?.Type == JTokenType.Object
                    ? new JObjectHelper((JObject)prop.Value).UnFlatten(propNames.Skip(1).FirstOrDefault())
                    : prop.Value;
            }
            return result;
        }

        public JObject UnFlatten(string name)
        {
            if (Source == null || string.IsNullOrEmpty(name))
            {
                return Source;
            }

            var names = name.Split(new[] { ':' }, 2);
            if (names.Length > 1)
            {
                return new JObject { [names[0]] = UnFlatten(names[1]) };
            }

            return new JObject { [name] = Source };
        }

        public JObject Assign(params JObject[] others)
        {
            if (Source == null)
            {
                return Source;
            }

            if (others?.Length > 0)
            {
                foreach (var other in others)
                {
                    foreach (var prop in other.Properties())
                    {
                        Source[prop.Name] = prop.Value;
                    }
                }
            }
            return Source;
        }

        public JObject Pick(params string[] colNames)
        {
            if (Source == null)
            {
                return Source;
            }

            var result = new JObject();
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

        public JObject Omit(params string[] colNames)
        {
            if (Source == null)
            {
                return Source;
            }

            var result = new JObject();
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
