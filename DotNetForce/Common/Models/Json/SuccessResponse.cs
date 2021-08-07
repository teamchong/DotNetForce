using Newtonsoft.Json;

namespace DotNetForce.Common.Models.Json
{
    public class SuccessResponse
    {
        [JsonProperty(PropertyName = "errors")]
        public object Errors;

        [JsonProperty(PropertyName = "id")] public string Id;

        [JsonProperty(PropertyName = "success")]
        public bool Success;
    }
}
