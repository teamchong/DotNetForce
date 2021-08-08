using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Serialization;

namespace DotNetForce.Common.Models.Json
{
    [XmlRoot(Namespace = "http://www.force.com/2009/06/asyncapi/dataload",
        ElementName = "error",
        IsNullable = false)]
    public class ErrorResponse
    {
        [XmlElement(ElementName = "exceptionCode")]
        [JsonProperty(PropertyName = "message")]
        public string? Message { get; set; }

        [XmlElement(ElementName = "exceptionMessage")]
        [JsonProperty(PropertyName = "errorCode")]
        public string? ErrorCode { get; set; }

        [XmlElement(ElementName = "exceptionStatusCode")]
        [JsonProperty(PropertyName = "statusCode")]
        public string? StatusCode { get; set; }

        [XmlElement(ElementName = "exceptionFields")]
        [JsonProperty(PropertyName = "fields")]
        // ReSharper disable once InconsistentNaming
        public string[]? fields { get; set; }
        
        public static ErrorResponses TryCast(JToken? token)
        {
            var unknownError = new ErrorResponse
            {
                ErrorCode = "UNKNOWN",
                Message = token?.ToString()
            };
            try
            {
                return (token?.Type == JTokenType.Array ?
                           token.ToObject<ErrorResponses>() :
                           new ErrorResponses { token?.ToObject<ErrorResponse>() ?? unknownError }) ??
                       new ErrorResponses { unknownError };
            }
            catch
            {
                return new ErrorResponses { unknownError };
            }
        }
    }
}
