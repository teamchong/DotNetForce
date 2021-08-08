using System.Linq;
using Newtonsoft.Json.Linq;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace DotNetForce
{
    internal class JObjectHelper
    {
        internal JObjectHelper(JObject? src)
        {
            Source = src ?? new JObject();
        }

        private JObject Source { get; set; }

        public JObject UnFlatten()
        {
            var result = new JObject();
            foreach (var prop in Source.Properties())
            {
                var propNames = prop.Name.Split(new[] { ':' }, 2);
                result[propNames[0]] = prop.Value.Type == JTokenType.Object
                    ? new JObjectHelper((JObject)prop.Value).UnFlatten(propNames.Skip(1).FirstOrDefault() ?? string.Empty)
                    : prop.Value;
            }
            return result;
        }

        public JObject UnFlatten(string name)
        {
            var names = name.Split(new[] { ':' }, 2);
            return names.Length > 1 ? new JObject { [names[0]] = UnFlatten(names[1]) } : new JObject { [name] = Source };
        }

        public JObject Assign(params JObject[] others)
        {
            if (others.Length <= 0) return Source;
            foreach (var other in others)
            foreach (var prop in other.Properties())
                Source[prop.Name] = prop.Value;
            return Source;
        }

        public JObject Pick(params string[] colNames)
        {
            var result = new JObject();
            if (colNames.Length == 0)
                foreach (var prop in Source.Properties())
                    result[prop.Name] = prop.Value;
            else
                foreach (var prop in Source.Properties())
                    if (colNames != null && colNames.Contains(prop.Name))
                        result[prop.Name] = prop.Value;
            return result;
        }

        public JObject Omit(params string[] colNames)
        {
            var result = new JObject();
            if (colNames.Length == 0)
                foreach (var prop in Source.Properties())
                    result[prop.Name] = prop.Value;
            else
                foreach (var prop in Source.Properties())
                    if (colNames != null && !colNames.Contains(prop.Name))
                        result[prop.Name] = prop.Value;
            return result;
        }
    }
}
