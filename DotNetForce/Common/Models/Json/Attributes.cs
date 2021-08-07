using Newtonsoft.Json;

namespace DotNetForce.Common.Models.Json
{
    public class AttributeInfo
    {
        [JsonProperty(PropertyName = "type")]
        public string TypeName { get; set; }

        [JsonProperty(PropertyName = "referenceId")]
        public string ReferenceId { get; set; }
    }
}
