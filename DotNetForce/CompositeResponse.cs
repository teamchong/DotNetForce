using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using DotNetForce.Force;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace DotNetForce
{
    public class CompositeResponse
    {
        [JsonProperty("body", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken Body { get; set; }
        
        [JsonProperty("httpHeaders", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken HttpHeaders { get; set; }
        
        [JsonProperty("httpStatusCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? HttpStatusCode { get; set; }

        [JsonProperty("referenceId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ReferenceId { get; set; }

        public JToken[] GetErrors()
        {
            if (Body?.Type == JTokenType.Array)
            {
                return Body.ToArray();
            }
            return null;
        }

        public JObject NullIfError()
        {
            if (Body?.Type == JTokenType.Array)
            {
                return null;
            }
            else if (Body.Type == JTokenType.Object)
            {
                return (JObject)Body;
            }
            return new JObject
            {
                ["message"] = Body
            };
        }

        public override string ToString()
        {
            return NullIfError()?.ToString() ?? GetErrors().ToString();
        }
    }
}
