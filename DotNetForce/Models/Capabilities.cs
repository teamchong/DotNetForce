using Newtonsoft.Json;

namespace DotNetForce.Chatter.Models
{
    public class Capabilities
    {
        [JsonProperty(PropertyName = "content")]
        public Content Content { get; set; }
    }
}
