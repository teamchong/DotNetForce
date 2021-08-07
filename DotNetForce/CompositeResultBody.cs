using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce
{
    public class CompositeResultBody
    {
        [JsonProperty("compositeResponse", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IList<CompositeSubRequestResult> CompositeResponse { get; set; }
    }
}
