using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce.Chatter.Models
{
    public class CommentPage
    {
        [JsonProperty(PropertyName = "comments")]
        public IList<Comment>? Comments { get; set; }

        [JsonProperty(PropertyName = "currentPageUrl")]
        public string? CurrentPageUrl { get; set; }

        [JsonProperty(PropertyName = "nextPageUrl")]
        public string? NextPageUrl { get; set; }

        [JsonProperty(PropertyName = "total")]
        public int Total { get; set; }
    }
}
