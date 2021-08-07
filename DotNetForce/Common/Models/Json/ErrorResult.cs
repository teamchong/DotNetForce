using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce.Common.Models.Json
{
    public class ErrorResult
    {
        [JsonProperty(PropertyName = "statusCode")]
        public string StatusCode { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "fields")]
        public IList<string> Fields { get; set; }
    }
}
