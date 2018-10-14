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
    public class BatchResultBody
    {
        [JsonProperty("hasErrors", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool HasErrors { get; set; }

        [JsonProperty("results", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<BatchSubrequestResult> Results { get; set; }
    }
}
