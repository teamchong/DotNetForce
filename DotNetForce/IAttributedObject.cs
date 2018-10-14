using Newtonsoft.Json;
using DotNetForce.Common.Models.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetForce.Common
{
    /// <summary>
    /// Interface enforcing implementation of Attributes Property for multiple record updates
    /// </summary>
    public interface IAttributedObject
    {
        [JsonProperty(PropertyName = "attributes")]
        ObjectAttributes Attributes { get; set; }
    }
}
