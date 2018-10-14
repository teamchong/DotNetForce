using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce.Chatter.Models
{
    public class TopicCollection
    {
        [JsonProperty(PropertyName = "currentPageUrl")]
        public string CurrentPageUrl { get; set; }

        [JsonProperty(PropertyName = "nextPageUrl")]
        public string NextPageUrl { get; set; }

        [JsonProperty(PropertyName = "topics")]
        public List<Topic> Topics { get; set; }
    }
}