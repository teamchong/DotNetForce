using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace DotNetForce.Common.Models.Json
{
    public class SaveResponse
    {
        [JsonProperty(PropertyName = "hasErrors")]
        public bool HasErrors { get; set; }

        [JsonProperty(PropertyName = "results")]
        public IList<SaveResult>? Results { get; set; }

        public SaveResponse Assert()
        {
            if (!HasErrors) return this;
            if (Results?.Count > 0)
            {
                var messages = Results.Select(JsonConvert.SerializeObject);
                throw new ForceException(Error.Unknown, string.Join(Environment.NewLine, messages));
            }
            else
            {
                var messages = Results?.SelectMany(r => r.Errors?.Select(err => err.Message) ?? Array.Empty<string>());
                throw new ForceException(Error.Unknown, string.Join(Environment.NewLine, messages ?? Array.Empty<string>()));
            }
        }
    }
}
