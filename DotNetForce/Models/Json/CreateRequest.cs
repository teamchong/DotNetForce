using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetForce.Common.Models.Json
{
    public class CreateRequest
    {
        [JsonProperty(PropertyName = "records")]
        public List<IAttributedObject> Records { get; set; }
    }
}
