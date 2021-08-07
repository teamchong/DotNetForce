using Newtonsoft.Json;

namespace DotNetForce.Chatter.Models
{
    public class ObjectFeedItemInput : FeedItemInput
    {
        [JsonProperty(PropertyName = "feedElementType")]
        public readonly string FeedItem = "FeedItem";

        [JsonProperty(PropertyName = "subjectId")]
        public string ObjectId { get; set; }

        [JsonProperty(PropertyName = "capabilities")]
        public Capabilities Capabilities { get; set; }
    }
}
