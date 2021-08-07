using Newtonsoft.Json;

namespace DotNetForce.Chatter.Models
{
    public class ModerationFlags
    {
        [JsonProperty(PropertyName = "flagCount")]
        public int FlagCount { get; set; }

        [JsonProperty(PropertyName = "flaggedByMe")]
        public bool FlaggedByMe { get; set; }
    }
}
