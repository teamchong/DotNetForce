using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace DotNetForce
{
    public class CompositeResponse
    {
        [JsonProperty("body", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken? Body { get; set; }

        [JsonProperty("httpHeaders", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken? HttpHeaders { get; set; }

        [JsonProperty("httpStatusCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? HttpStatusCode { get; set; }

        [JsonProperty("referenceId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? ReferenceId { get; set; }

        public JToken[]? GetErrors()
        {
            return Body?.Type == JTokenType.Array ? Body.ToArray() : null;
        }

        public JObject? NullIfError()
        {
            return Body?.Type switch
            {
                JTokenType.Array => null,
                JTokenType.Object => (JObject)Body,
                _ => new JObject { ["message"] = Body }
            };
        }

        public override string ToString()
        {
            return NullIfError()?.ToString() ?? string.Empty;
        }
    }
}
