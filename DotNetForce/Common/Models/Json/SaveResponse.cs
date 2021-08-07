using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce.Common.Models.Json
{
    public class SaveResponse
    {
        [JsonProperty(PropertyName = "hasErrors")]
        public bool HasErrors { get; set; }

        [JsonProperty(PropertyName = "results")]
        public IList<SaveResult> Results { get; set; }
    }
}
