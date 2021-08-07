using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetForce
{
    public class CompositeSubRequestResult
    {
        [JsonProperty("body", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken Body { get; set; }

        [JsonProperty("httpHeaders", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public HttpHeaders HttpHeaders { get; set; }

        [JsonProperty("httpStatusCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int HttpStatusCode { get; set; }

        [JsonProperty("referenceId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ReferenceId { get; set; }
    }
}
