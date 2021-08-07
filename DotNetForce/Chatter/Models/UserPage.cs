using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce.Chatter.Models
{
    public class UserPage
    {
        [JsonProperty(PropertyName = "currentPageUrl")]
        public string CurrentPageUrl { get; set; }

        [JsonProperty(PropertyName = "nextPageUrl")]
        public string NextPageUrl { get; set; }

        [JsonProperty(PropertyName = "previousPageUrl")]
        public string PreviousPageUrl { get; set; }

        [JsonProperty(PropertyName = "users")]
        public IList<UserDetail> Users { get; set; }
    }
}
