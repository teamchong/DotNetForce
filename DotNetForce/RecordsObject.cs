using System.Collections.Generic;
using System.Linq;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

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
            foreach (var item in enumerable) Records.Add(item);
            AdditionalData = new JObject();
        }

        [JsonProperty(PropertyName = "records", NullValueHandling = NullValueHandling.Ignore)]
        public IList<IAttributedObject> Records { get; set; }

        [JsonExtensionData]
        public JObject AdditionalData { get; set; }

        public JToken this[string propertyName]
        {
            get => AdditionalData[propertyName] ?? JValue.CreateNull();
            set => AdditionalData[propertyName] = value;
        }

        public CreateRequest ToCreateRequest()
        {
            return new CreateRequest
            {
                Records = Records.ToList()
            };
        }

        public static implicit operator JObject(RecordsObject obj)
        {
            var dict = new JObject(obj.AdditionalData) { ["records"] = JToken.FromObject(obj.Records) };
            return dict;
        }
    }
}
