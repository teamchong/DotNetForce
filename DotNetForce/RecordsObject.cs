using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotNetForce
{
    public class RecordsObject
    {
        public RecordsObject()
        {
            Records = new List<IAttributedObject>();
            AdditionalData = JToken.FromObject(new Dictionary<string, JToken>());
        }

        public RecordsObject(IEnumerable<IAttributedObject> enumerable)
        {
            Records = new List<IAttributedObject>();
            foreach (var item in enumerable)
            {
                Records.Add(item);
            }
            AdditionalData = JToken.FromObject(new Dictionary<string, JToken>());
        }

        [JsonProperty(PropertyName = "records", NullValueHandling = NullValueHandling.Ignore)]
        public List<IAttributedObject> Records { get; set; }

        [JsonExtensionData]
        public JToken AdditionalData { get; set; }

        public CreateRequest ToCreateRequest()
        {
            return new CreateRequest
            {
                Records = Records.Cast<IAttributedObject>().ToList()
            };
        }
        
        public JToken this[string propertyName]
        {
            get => AdditionalData[propertyName];
            set => AdditionalData[propertyName] = value;
        }

        public static implicit operator JToken(RecordsObject obj)
        {
            var dict = new Dictionary<string, JToken>((IDictionary<string, JToken>)obj.AdditionalData);
            if (dict.ContainsKey("attributes"))
            {
                dict["records"] = JToken.FromObject(obj.Records);
            }
            else
            {
                dict.Add("records", JToken.FromObject(obj.Records));
            }
            return JToken.FromObject(dict);
        }
    }
}
