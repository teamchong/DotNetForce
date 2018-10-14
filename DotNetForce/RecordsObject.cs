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
            AdditionalData = new JObject();
        }

        public RecordsObject(IEnumerable<IAttributedObject> enumerable)
        {
            Records = new List<IAttributedObject>();
            foreach (var item in enumerable)
            {
                Records.Add(item);
            }
            AdditionalData = new JObject();
        }

        [JsonProperty(PropertyName = "records", NullValueHandling = NullValueHandling.Ignore)]
        public List<IAttributedObject> Records { get; set; }

        [JsonExtensionData]
        public JObject AdditionalData { get; set; }

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

        public static implicit operator JObject(RecordsObject obj)
        {
            return new JObject(obj.AdditionalData) { ["records"] = JArray.FromObject(obj.Records) };
        }
    }
}
