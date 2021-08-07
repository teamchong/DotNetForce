﻿using System.Xml.Serialization;
using Newtonsoft.Json;

namespace DotNetForce.Common.Models.Json
{
    [XmlRoot(Namespace = "http://www.force.com/2009/06/asyncapi/dataload",
        ElementName = "error",
        IsNullable = false)]
    public class ErrorResponse
    {
        [XmlElement(ElementName = "exceptionCode")] [JsonProperty(PropertyName = "message")]
        public string Message;

        [XmlElement(ElementName = "exceptionMessage")] [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode;

        [XmlElement(ElementName = "exceptionStatusCode")] [JsonProperty(PropertyName = "statusCode")]
        public string StatusCode;

        [XmlElement(ElementName = "exceptionFields")] [JsonProperty(PropertyName = "fields")]
        // ReSharper disable once InconsistentNaming
        public string[] fields;
    }
}
