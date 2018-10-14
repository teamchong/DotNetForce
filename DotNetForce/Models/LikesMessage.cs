using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce.Chatter.Models
{
    public class LikeMessageBody
    {
        [JsonProperty(PropertyName = "messageSegments")]
        public List<MessageSegment> MessageSegments { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}