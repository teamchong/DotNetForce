using Newtonsoft.Json;

namespace DotNetForce.Chatter.Models
{
    public class FeedItemInput
    {
        [JsonProperty(PropertyName = "attachment")]
        public Attachment? Attachment { get; set; }

        [JsonProperty(PropertyName = "body")]
        public MessageBodyInput? Body { get; set; }

        [JsonProperty(PropertyName = "subjectId")]
        public string? SubjectId { get; set; }

        [JsonProperty(PropertyName = "feedElementType")]
        public string? FeedElementType { get; set; }
    }
}
