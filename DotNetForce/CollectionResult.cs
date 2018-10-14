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
    public class CollectionResult
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("success", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Success { get; set; }

        [JsonProperty("errors", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<CollectionError> Errors { get; set; }
    }
}
