using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;

namespace DotNetForce.Common
{
    /// <summary>
    ///     Interface enforcing implementation of Attributes Property for multiple record updates
    /// </summary>
    public interface IAttributedObject
    {
        [JsonProperty(PropertyName = "attributes")]
        ObjectAttributes Attributes { get; set; }
    }
}
