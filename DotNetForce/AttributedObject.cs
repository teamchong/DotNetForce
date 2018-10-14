using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

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
            AdditionalData = new JObject(other);
        }
        public AttributedObject(string type, string referenceId, JObject other)
        {
            Attributes = new ObjectAttributes { Type = type, ReferenceId = referenceId };
            AdditionalData = new JObject(other);
        }
        public AttributedObject(string type, object content)
        {
            Attributes = new ObjectAttributes { Type = type };
            AdditionalData = new JObject(content);
        }
        public AttributedObject(string type, string referenceId, object content)
        {
            Attributes = new ObjectAttributes { Type = type, ReferenceId = referenceId };
            AdditionalData = new JObject(content);
        }
        public AttributedObject(string type, params object[] content)
        {
            Attributes = new ObjectAttributes { Type = type };
            AdditionalData = new JObject(content);
        }
        public AttributedObject(string type, string referenceId, params object[] content)
        {
            Attributes = new ObjectAttributes { Type = type, ReferenceId = referenceId };
            AdditionalData = new JObject(content);
        }

        [JsonProperty(PropertyName = "attributes", NullValueHandling = NullValueHandling.Ignore)]
        public ObjectAttributes Attributes { get; set; }

        [JsonExtensionData]
        public JObject AdditionalData { get; set; }
        
        public JToken this[string propertyName]
        {
            get => AdditionalData[propertyName];
            set => AdditionalData[propertyName] = value;
        }

        public static implicit operator JObject(AttributedObject obj)
        {
            return new JObject(obj.AdditionalData) { ["attributes"] = JObject.FromObject(obj.Attributes) };
        }
    }
}
