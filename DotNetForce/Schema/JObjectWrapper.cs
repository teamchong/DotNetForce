using DotNetForce;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetForce
{
    public class JObjectWrapper : IAttributedObject
    {
        [JsonExtensionData]
        protected JToken Object { get; set; }

        [JsonIgnore]
        public ObjectAttributes Attributes { get => Object?["attributes"]?.ToObject<ObjectAttributes>(); set => Object["attributes"] = value == null ? null : JToken.FromObject(value); }

        public JObjectWrapper(JToken obj) => Object = obj;

        public JObjectWrapper Spread()
        {
            return Spread(":");
        }

        public JObjectWrapper Spread(string sep)
        {
            var result = JToken.FromObject(new Dictionary<string, JToken>());
            if (Object != null)
            {
                foreach (var prop in (IDictionary<string, JToken>)Object)
                {
                    var splits = prop.Key.Split(new[]{ sep }, 2, StringSplitOptions.None);
                    if (splits.Length == 1)
                    {
                        result[prop.Key] = prop.Value;
                    }
                    else if (result[prop.Key]?.Type == JTokenType.Object)
                    {
                        var subObj = result[prop.Key];
                        subObj[splits[1]] = prop.Value;
                        result[splits[0]] = new JObjectWrapper(subObj).Spread();
                    }
                    else
                    {
                        result[splits[0]] = new JObjectWrapper(JToken.FromObject(new Dictionary<string, JToken> { [splits[1]] = prop.Value })).Spread();
                    }
                }
            }
            return new JObjectWrapper(result);
        }

        public JToken Unwrap() => Object;
        
        public JToken Unwrap(SfObjectBase type) => Unwrap(type?.ToString());

        public JToken Unwrap(string type)
        {
            if (Object != null) Object["attributes"] = JToken.FromObject(new Dictionary<string, JToken> { ["type"] = type });
            return Object;
        }
        
        public JToken Unwrap(SfObjectBase type, string referenceId) => Unwrap(type?.ToString(), referenceId);

        public JToken Unwrap(string type, string referenceId)
        {
            if (Object != null) Object["attributes"] = JToken.FromObject(new Dictionary<string, JToken> { ["type"] = type, ["referenceId"] = referenceId });
            return Object;
        }

        public JToken Get(string path)
        {
            var paths = path?.Split(new[]{'.' }, 2);
            if (paths?.Length == 1)
            {
                return Object[paths[0]];
            }
            else if (paths?.Length > 1)
            {
                if (Object[paths[0]]?.Type == JTokenType.Object)
                {
                    return new JObjectWrapper(Object[paths[0]]).Get(paths[1]);
                }
            }
            return null;
        }

        public JObjectWrapper Set(string path, JToken value)
        {
            var paths = path.Split(new[]{'.' }, 2);
            if (paths.Length == 1)
            {
                Object[paths[0]] = value;
            }
            else if (paths.Length > 1)
            {
                if (Object[paths[0]]?.Type != JTokenType.Object)
                {
                    Object[paths[0]] = JToken.FromObject(new Dictionary<string, JToken>());
                }
                return Set(paths[1], value);
            }
            return this;
        }


        public JToken Get<T>(SfAddressField<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString());
        }

        public JObjectWrapper Set<T>(SfAddressField<T> field, JToken value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public JToken Get<T>(SfAnyTypeField<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString());
        }

        public JObjectWrapper Set<T>(SfAnyTypeField<T> field, JToken value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public string Get<T>(SfBase64Field<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfBase64Field<T> field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public bool? Get<T>(SfBooleanField<T> field) where T : SfObjectBase
        {
            return (bool?)Get(field?.ToString());
        }

        public JObjectWrapper Set<T>(SfBooleanField<T> field, bool? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }


        public QueryResult<JToken> Get<T, TChild>(SfChildRelationship<T, TChild> field)
            where T : SfObjectBase, new() where TChild : SfObjectBase, new()
        {
            return Get(field?.ToString())?.ToObject<QueryResult<JToken>>();
        }

        public JObjectWrapper Set<T, TChild>(SfChildRelationship<T, TChild> field, JToken value)
            where T : SfObjectBase, new() where TChild: SfObjectBase, new()
        {
            return Set(field?.ToString(), value);
        }
        

        public string Get<T>(SfComboBoxField<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfComboBoxField<T> field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public JToken Get<T>(SfComplexValueField<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString());
        }

        public JObjectWrapper Set<T>(SfComplexValueField<T> field, JToken value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public double? Get<T>(SfCurrencyField<T> field) where T : SfObjectBase
        {
            return (double?)Get(field?.ToString());
        }

        public JObjectWrapper Set<T>(SfCurrencyField<T> field, double? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }

        public JObjectWrapper Set<T>(SfCurrencyField<T> field, decimal? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public DateTime? Get<T>(SfDateField<T> field) where T : SfObjectBase
        {
            return DNF.ToDateTime(Get(field?.ToString())?.ToString());
        }

        public JObjectWrapper Set<T>(SfDateField<T> field, DateTime? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), DNF.SOQLDate(value));
        }
        

        public DateTime? Get<T>(SfDateTimeField<T> field) where T : SfObjectBase
        {
            return DNF.ToDateTime(Get(field?.ToString())?.ToString());
        }

        public JObjectWrapper Set<T>(SfDateTimeField<T> field, DateTime? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), DNF.SOQLDateTime(value));
        }
        

        public double? Get<T>(SfDoubleField<T> field) where T : SfObjectBase
        {
            return (double?)Get(field?.ToString());
        }

        public JObjectWrapper Set<T>(SfDoubleField<T> field, double? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }

        public JObjectWrapper Set<T>(SfDoubleField<T> field, decimal? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public string Get<T>(SfEmailField<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfEmailField<T> field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public string Get<T>(SfIdField<T> field) where T : SfObjectBase
        {
            return DNF.ToID18(Get(field?.ToString())?.ToString());
        }

        public JObjectWrapper Set<T>(SfIdField<T> field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), DNF.ToID15(value));
        }
        

        public long? Get<T>(SfIntField<T> field) where T : SfObjectBase
        {
            return (long?)Get(field?.ToString());
        }

        public JObjectWrapper Set<T>(SfIntField<T> field, long? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public JToken Get<T>(SfLocationField<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString());
        }

        public JObjectWrapper Set<T>(SfLocationField<T> field, JToken value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public string Get<T>(SfMultiPicklistField<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfMultiPicklistField<T> field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public double? Get<T>(SfPercentField<T> field) where T : SfObjectBase
        {
            return (double?)Get(field?.ToString());
        }

        public JObjectWrapper Set<T>(SfPercentField<T> field, double? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }

        public JObjectWrapper Set<T>(SfPercentField<T> field, decimal? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public string Get<T>(SfPhoneField<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfPhoneField<T> field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public string Get<T>(SfPicklistField<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfPicklistField<T> field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public string Get<T>(SfStringField<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfStringField<T> field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public string Get<T>(SfTextAreaField<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfTextAreaField<T> field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }
        

        public DateTime? Get<T>(SfTimeField<T> field) where T : SfObjectBase
        {
            return DNF.FromSOQLTime(Get(field?.ToString())?.ToString());
        }

        public JObjectWrapper Set<T>(SfTimeField<T> field, DateTime? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), DNF.SOQLTime(value));
        }
        

        public string Get<T>(SfUrlField<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfUrlField<T> field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }

        public override string ToString()
        {
            return Object?.ToString();
        }

        public string ToString(Formatting formatting, params JsonConverter[] converters)
        {
            return Object?.ToString(formatting, converters);
        }

        public static implicit operator JObjectWrapper(JToken obj) => new JObjectWrapper(obj);
        public static implicit operator JToken(JObjectWrapper wrapper) => wrapper.Unwrap();
    }
}
