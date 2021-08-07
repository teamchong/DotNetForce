using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetForce
{
    public class BatchSubRequestResult
    {
        [JsonProperty("result", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken Result { get; set; }

        [JsonProperty("statusCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int StatusCode { get; set; }
    }
}
