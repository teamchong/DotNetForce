using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce.Chatter.Models
{
    public class FeedBody
    {
        [JsonProperty(PropertyName = "messageSegments")]
        public IList<MessageSegment>? MessageSegments { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string? Text { get; set; }
    }
}
