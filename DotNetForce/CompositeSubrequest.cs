using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetForce
{
    public class CompositeSubRequest
    {
        [JsonIgnore] public string ResponseType = "object";

        [JsonProperty("body", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken Body { get; set; }

        [JsonProperty("httpHeaders", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public HttpHeaders HttpHeaders { get; set; }

        [JsonProperty("method", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Method { get; set; }

        [JsonProperty("referenceId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ReferenceId { get; set; }

        [JsonProperty("url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Url { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
