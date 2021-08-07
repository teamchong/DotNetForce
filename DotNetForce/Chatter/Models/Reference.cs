using Newtonsoft.Json;

namespace DotNetForce.Chatter.Models
{
    public class Reference
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}