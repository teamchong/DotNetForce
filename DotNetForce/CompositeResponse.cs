using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetForce
{
    public class CompositeResponse
    {
        [JsonProperty("body", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken Body { get; set; }

        [JsonProperty("httpHeaders", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken HttpHeaders { get; set; }

        [JsonProperty("httpStatusCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? HttpStatusCode { get; set; }

        [JsonProperty("referenceId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ReferenceId { get; set; }

        public JToken[] GetErrors()
        {
            if (Body?.Type == JTokenType.Array) return Body.ToArray();
            return null;
        }

        public JObject NullIfError()
        {
            if (Body?.Type == JTokenType.Array) return null;
            if (Body?.Type == JTokenType.Object) return (JObject)Body;
            return new JObject
            {
                ["message"] = Body
            };
        }

        public override string ToString()
        {
            return NullIfError()?.ToString() ?? GetErrors().ToString();
        }
    }
}
