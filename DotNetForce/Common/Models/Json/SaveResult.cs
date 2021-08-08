using Newtonsoft.Json;

namespace DotNetForce.Common.Models.Json
{
    public class SaveResult
    {
        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; }

        [JsonProperty(PropertyName = "referenceId")]
        public string? ReferenceId { get; set; }

        [JsonProperty(PropertyName = "errors")]
        public ErrorResult[]? Errors { get; set; }
    }
}
