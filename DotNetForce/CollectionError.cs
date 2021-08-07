using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce
{
    public class CollectionError
    {
        [JsonProperty("statusCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string StatusCode { get; set; }

        [JsonProperty("message", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("fields", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IList<string> Fields { get; set; }
    }
}
