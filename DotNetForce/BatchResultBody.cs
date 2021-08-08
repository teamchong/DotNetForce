using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce
{
    public class BatchResultBody
    {
        [JsonProperty("hasErrors", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool HasErrors { get; set; }

        [JsonProperty("results", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IList<BatchSubRequestResult>? Results { get; set; }
    }
}
