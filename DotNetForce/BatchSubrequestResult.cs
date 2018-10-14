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
    public class BatchSubrequestResult
    {
        [JsonProperty("result", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken Result { get; set; }
        
        [JsonProperty("statusCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int StatusCode { get; set; }
    }
}
