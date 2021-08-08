using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// ReSharper disable UnusedMember.Global

namespace DotNetForce
{
    public class AttributedObject : IAttributedObject
    {
        public AttributedObject(string type)
        {
            Attributes = new ObjectAttributes { Type = type };
            AdditionalData = new JObject();
        }

        public AttributedObject(string type, JObject other)
        {
            Attributes = new ObjectAttributes { Type = type };
            AdditionalData = other;
        }

        public AttributedObject(string type, string referenceId, JObject other)
        {
            Attributes = new ObjectAttributes { Type = type, ReferenceId = referenceId };
            AdditionalData = other;
        }

        public AttributedObject(string type, object content)
        {
            Attributes = new ObjectAttributes { Type = type };
            AdditionalData = JObject.FromObject(content);
        }

        public AttributedObject(string type, string referenceId)
        {
            Attributes = new ObjectAttributes { Type = type, ReferenceId = referenceId };
            AdditionalData = new JObject();
        }

        public AttributedObject(string type, string referenceId, object content)
        {
            Attributes = new ObjectAttributes { Type = type, ReferenceId = referenceId };
            AdditionalData = JObject.FromObject(content);
        }

        [JsonExtensionData]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public JObject AdditionalData { get; set; }
        
        public JToken this[string propertyName]
        {
            get => AdditionalData[propertyName] ?? JValue.CreateNull();
            set => AdditionalData[propertyName] = value;
        }

        [JsonProperty(PropertyName = "attributes", NullValueHandling = NullValueHandling.Ignore)]
        public ObjectAttributes Attributes { get; set; }

        public static implicit operator JObject(AttributedObject obj)
        {
            var dict = JObject.FromObject(obj.AdditionalData);
            if (dict.ContainsKey("attributes")) dict["attributes"] = JToken.FromObject(obj.Attributes);
            else dict.Add("attributes", JToken.FromObject(obj.Attributes));
            return dict;
        }
    }
}
