using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce.Chatter.Models
{
    public class MessageBodyInput
    {
        [JsonProperty(PropertyName = "messageSegments")]
        public List<MessageSegmentInput> MessageSegments { get; set; }
    }
}