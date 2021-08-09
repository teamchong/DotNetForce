using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce.Chatter.Models
{
    public class FeedItemTopics
    {
        [JsonProperty(PropertyName = "topics")]
        public IList<object>? Topics { get; set; }

        [JsonProperty(PropertyName = "canAssignTopics")]
        public bool CanAssignTopics { get; set; }
    }
}
