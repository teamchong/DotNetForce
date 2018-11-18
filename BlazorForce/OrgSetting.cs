using Microsoft.AspNetCore.Blazor.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BlazorForce
{
    public class OrgSetting
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AccessToken { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RefreshToken { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string InstanceUrl { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LoginUri { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public long? IssuedAt { get; set; }

        public string GetName()
        {
            if (InstanceUrl != null)
            {
                return Regex.Replace(Regex.Replace(InstanceUrl, @"^https?://([^.]+).*\.salesforce\.com.*$", @"$1"), "-+", " ").ToUpper();
            }
            return null;
        }

        //public DateTime? GetIssuedAt()
        //{
        //    if (IssuedAt != null)
        //    {
        //        return new DateTime(1970, 1, 1).AddTicks((long)IssuedAt * 10000);
        //    }
        //    return null;
        //}
    }
}