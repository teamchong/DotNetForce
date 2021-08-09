using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetForce
{
    public class BatchSubRequest
    {
        [JsonIgnore] public string ResponseType = "object";

        [JsonProperty("binaryPartName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? BinaryPartName { get; set; }

        [JsonProperty("binaryPartNameAlias", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? BinaryPartNameAlias { get; set; }

        [JsonProperty("method", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Method { get; set; }

        [JsonProperty("richInput", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken? RichInput { get; set; }

        [JsonProperty("url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Url { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
