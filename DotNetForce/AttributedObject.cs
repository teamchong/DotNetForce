using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace DotNetForce
{
    public class AttributedObject : IAttributedObject
    {

        public AttributedObject(string type)
        {
            Attributes = new ObjectAttributes { Type = type };
            AdditionalData = JToken.FromObject(new Dictionary<string, JToken>());
        }
        public AttributedObject(string type, JToken other)
        {
            Attributes = new ObjectAttributes { Type = type };
            AdditionalData = other;
        }
        public AttributedObject(string type, string referenceId, JToken other)
        {
            Attributes = new ObjectAttributes { Type = type, ReferenceId = referenceId };
            AdditionalData = other;
        }
        public AttributedObject(string type, object content)
        {
            Attributes = new ObjectAttributes { Type = type };
            AdditionalData = JToken.FromObject(content);
        }
        public AttributedObject(string type, string referenceId, object content)
        {
            Attributes = new ObjectAttributes { Type = type, ReferenceId = referenceId };
            AdditionalData = JToken.FromObject(content);
        }
        public AttributedObject(string type, params object[] content)
        {
            Attributes = new ObjectAttributes { Type = type };
            AdditionalData = JToken.FromObject(content);
        }
        public AttributedObject(string type, string referenceId, params object[] content)
        {
            Attributes = new ObjectAttributes { Type = type, ReferenceId = referenceId };
            AdditionalData = JToken.FromObject(content);
        }

        [JsonProperty(PropertyName = "attributes", NullValueHandling = NullValueHandling.Ignore)]
        public ObjectAttributes Attributes { get; set; }

        [JsonExtensionData]
        public JToken AdditionalData { get; set; }
        
        public JToken this[string propertyName]
        {
            get => AdditionalData[propertyName];
            set => AdditionalData[propertyName] = value;
        }

        public static implicit operator JToken(AttributedObject obj)
        {
            var dict = new Dictionary<string, JToken>((IDictionary<string, JToken>)obj.AdditionalData);
            if (dict.ContainsKey("attributes"))
            {
                dict["attributes"] = JToken.FromObject(obj.Attributes);
            }
            else
            {
                dict.Add("attributes", JToken.FromObject(obj.Attributes));
            }
            return JToken.FromObject(dict);
        }
    }
}
