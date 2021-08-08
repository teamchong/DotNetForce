using Newtonsoft.Json;

namespace DotNetForce.Common.Models.Json
{
    public class AuthToken
    {
        [JsonProperty(PropertyName = "access_token")]
        public string? AccessToken;

        [JsonProperty(PropertyName = "id")]
        public string? Id;

        [JsonProperty(PropertyName = "instance_url")]
        public string? InstanceUrl;

        [JsonProperty(PropertyName = "issued_at")]
        public string? IssuedAt;

        [JsonProperty(PropertyName = "refresh_token")]
        public string? RefreshToken;

        [JsonProperty(PropertyName = "signature")]
        public string? Signature;
    }
}
