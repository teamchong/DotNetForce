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
    public class CompositeSubrequest
    {
        [JsonIgnore]
        public string ResponseType = "object";

        [JsonProperty("body", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken Body { get; set; }
        
        [JsonProperty("httpHeaders", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public HttpHeaders HttpHeaders { get; set; }
        
        [JsonProperty("method", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Method { get; set; }

        [JsonProperty("referenceId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ReferenceId { get; set; }
        
        [JsonProperty("url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Url { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
