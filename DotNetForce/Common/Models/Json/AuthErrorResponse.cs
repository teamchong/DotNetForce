using Newtonsoft.Json;

namespace DotNetForce.Common.Models.Json
{
    public class AuthErrorResponse
    {
        [JsonProperty(PropertyName = "error")] public string Error;

        [JsonProperty(PropertyName = "error_description")]
        public string ErrorDescription;
    }
}
