using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce.Common.Models.Json
{
    public class CreateRequest
    {
        [JsonProperty(PropertyName = "records")]
        public IList<IAttributedObject> Records { get; set; }
    }
}
