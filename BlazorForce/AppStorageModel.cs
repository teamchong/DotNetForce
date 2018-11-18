using Microsoft.AspNetCore.Blazor.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace BlazorForce
{
    public class AppStorageModel
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, OrgSetting> Organizations { get; set; }

        public AppStorageModel()
        {
            Organizations = new Dictionary<string, OrgSetting>();
        }
    }
}