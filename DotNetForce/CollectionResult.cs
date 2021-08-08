using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce
{
    public class CollectionResult
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Id { get; set; }

        [JsonProperty("success", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Success { get; set; }

        [JsonProperty("errors", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IList<CollectionError>? Errors { get; set; }
    }
}
