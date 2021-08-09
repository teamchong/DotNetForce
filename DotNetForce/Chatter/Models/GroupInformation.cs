using Newtonsoft.Json;

namespace DotNetForce.Chatter.Models
{
    public class GroupInformation
    {
        [JsonProperty(PropertyName = "text")]
        public string? Text { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string? Title { get; set; }
    }
}
