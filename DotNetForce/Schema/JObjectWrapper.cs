using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetForce.Schema
{
    [JetBrains.Annotations.PublicAPI]
    public class JObjectWrapper : IAttributedObject
    {
        public JObjectWrapper() { Object = new JObject(); }

        public JObjectWrapper(SfObjectBase type) : this(new JObject(), type?.ToString()) { }

        public JObjectWrapper(SfObjectBase type, string referenceId) : this(new JObject(), type?.ToString(), referenceId) { }

        public JObjectWrapper(string type) : this(new JObject(), type) { }

        public JObjectWrapper(string type, string referenceId) : this(new JObject(), type, referenceId) { }

        public JObjectWrapper(JObject obj) { Object = obj; }

        public JObjectWrapper(JObject obj, SfObjectBase type) : this(obj, type?.ToString()) { }

        public JObjectWrapper(JObject obj, string type)
        {
            if (obj != null) obj["attributes"] = new JObject { ["type"] = type };
            Object = obj;
        }

        public JObjectWrapper(JObject obj, SfObjectBase type, string referenceId) : this(obj, type?.ToString(), referenceId) { }

        public JObjectWrapper(JObject obj, string type, string referenceId)
        {
            if (obj != null) obj["attributes"] = new JObject { ["type"] = type, ["referenceId"] = referenceId };
            Object = obj;
        }

        [JsonExtensionData]
        protected JObject Object { get; set; }

        [JsonIgnore]
        public ObjectAttributes Attributes { get => Object?["attributes"]?.ToObject<ObjectAttributes>(); set => Object["attributes"] = value == null ? null : JObject.FromObject(value); }

        public JObjectWrapper Spread()
        {
            return Spread(":");
        }

        public JObjectWrapper Spread(string sep)
        {
            var result = new JObject();
            if (Object != null)
                foreach (var prop in Object.Properties())
                {
                    var splits = prop.Name.Split(new[] { sep }, 2, StringSplitOptions.None);
                    if (splits.Length == 1)
                    {
                        result[prop.Name] = prop.Value;
                    }
                    else if (result[prop.Name]?.Type == JTokenType.Object)
                    {
                        var subObj = result[prop.Name];
                        subObj[splits[1]] = prop.Value;
                        result[splits[0]] = new JObjectWrapper((JObject)subObj).Spread();
                    }
                    else
                    {
                        result[splits[0]] = new JObjectWrapper(new JObject { [splits[1]] = prop.Value }).Spread();
                    }
                }
            return new JObjectWrapper(result);
        }

        public JObject Unwrap()
        {
            return Object;
        }

        public JObject Unwrap(SfObjectBase type)
        {
            return Unwrap(type?.ToString());
        }

        public JObject Unwrap(string type)
        {
            if (Object != null) Object["attributes"] = new JObject { ["type"] = type };
            return Object;
        }

        public JObject Unwrap(SfObjectBase type, string referenceId)
        {
            return Unwrap(type?.ToString(), referenceId);
        }

        public JObject Unwrap(string type, string referenceId)
        {
            if (Object != null) Object["attributes"] = new JObject { ["type"] = type, ["referenceId"] = referenceId };
            return Object;
        }

        public JToken Get(string path)
        {
            var paths = path?.Split(new[] { '.' }, 2);
            if (paths?.Length == 1) return Object[paths[0]];
            if (paths?.Length > 1)
                if (Object[paths[0]]?.Type == JTokenType.Object)
                    return new JObjectWrapper((JObject)Object[paths[0]]).Get(paths[1]);
            return null;
        }

        public JObjectWrapper Set(string path, JToken value)
        {
            var paths = path.Split(new[] { '.' }, 2);
            if (paths.Length == 1)
            {
                Object[paths[0]] = value;
            }
            else if (paths.Length > 1)
            {
                if (Object[paths[0]]?.Type != JTokenType.Object) Object[paths[0]] = new JObject();
                new JObjectWrapper((JObject)Object[paths[0]]).Set(paths[1], value);
            }
            return this;
        }


        public JToken Get<T>(SfAddressField<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString());
        }

        public JObjectWrapper Set<T>(SfAddressField<T> field, JObject value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }


        public JToken Get<T>(SfAnyTypeField<T> field) where T : SfObjectBase
        {
            return Get(field?.ToString());
        }

        public JObjectWrapper Set<T>(SfAnyTypeField<T> field, JObject value) where T : SfObjectBase
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


        public QueryResult<JObject> Get<T, TChild>(SfChildRelationship<TChild> field)
            where T : SfObjectBase, new() where TChild : SfObjectBase, new()
        {
            return Get(field?.ToString())?.ToObject<QueryResult<JObject>>();
        }

        public JObjectWrapper Set<T, TChild>(SfChildRelationship<TChild> field, JObject value)
            where T : SfObjectBase, new() where TChild : SfObjectBase, new()
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

        public JObjectWrapper Set<T>(SfComplexValueField<T> field, JObject value) where T : SfObjectBase
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
            return Dnf.ToDateTime(Get(field?.ToString())?.ToString());
        }

        public JObjectWrapper Set<T>(SfDateField<T> field, DateTime? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), Dnf.SoqlDate(value));
        }


        public DateTime? Get<T>(SfDateTimeField<T> field) where T : SfObjectBase
        {
            return Dnf.ToDateTime(Get(field?.ToString())?.ToString());
        }

        public JObjectWrapper Set<T>(SfDateTimeField<T> field, DateTime? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), Dnf.SoqlDateTime(value));
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
            return Dnf.ToId18(Get(field?.ToString())?.ToString());
        }

        public JObjectWrapper Set<T>(SfIdField<T> field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), Dnf.ToId15(value));
        }


        public long? Get<T>(SfIntField field) where T : SfObjectBase
        {
            return (long?)Get(field?.ToString());
        }

        public JObjectWrapper Set<T>(SfIntField field, long? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }


        public JToken Get<T>(SfLocationField field) where T : SfObjectBase
        {
            return Get(field?.ToString());
        }

        public JObjectWrapper Set<T>(SfLocationField field, JObject value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }


        public string Get<T>(SfMultiPicklistField field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfMultiPicklistField field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }


        public double? Get<T>(SfPercentField field) where T : SfObjectBase
        {
            return (double?)Get(field?.ToString());
        }

        public JObjectWrapper Set<T>(SfPercentField field, double? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }

        public JObjectWrapper Set<T>(SfPercentField field, decimal? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }


        public string Get<T>(SfPhoneField field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfPhoneField field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }


        public string Get<T>(SfPicklistField field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfPicklistField field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }


        public string Get<T>(SfStringField field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfStringField field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }


        public string Get<T>(SfTextAreaField field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfTextAreaField field, string value) where T : SfObjectBase
        {
            return Set(field?.ToString(), value);
        }


        public DateTime? Get<T>(SfTimeField field) where T : SfObjectBase
        {
            return Dnf.FromSoqlTime(Get(field?.ToString())?.ToString());
        }

        public JObjectWrapper Set<T>(SfTimeField field, DateTime? value) where T : SfObjectBase
        {
            return Set(field?.ToString(), Dnf.SoqlTime(value));
        }


        public string Get<T>(SfUrlField field) where T : SfObjectBase
        {
            return Get(field?.ToString())?.ToString();
        }

        public JObjectWrapper Set<T>(SfUrlField field, string value) where T : SfObjectBase
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

        public static implicit operator JObjectWrapper(JObject obj)
        {
            return new JObjectWrapper(obj);
        }

        public static implicit operator JObject(JObjectWrapper wrapper)
        {
            return wrapper.Unwrap();
        }
        public static JObjectWrapper Wrap(JObject obj) { return new JObjectWrapper(obj); }
        public static JObjectWrapper Wrap(JObject obj, SfObjectBase type) { return new JObjectWrapper(obj, type); }
        public static JObjectWrapper Wrap(JObject obj, string type) { return new JObjectWrapper(obj, type); }
        public static JObjectWrapper Wrap(JObject obj, SfObjectBase type, string referenceId) { return new JObjectWrapper(obj, type, referenceId); }
        public static JObjectWrapper Wrap(JObject obj, string type, string referenceId) { return new JObjectWrapper(obj, type, referenceId); }
        public static async Task<IEnumerable<JObjectWrapper>> Wrap(Task<IEnumerable<JObject>> objects) { return Wrap(await objects); }
        public static async Task<IEnumerable<JObjectWrapper>> Wrap(Task<IEnumerable<JObject>> objects, SfObjectBase type) { return Wrap(await objects, type); }
        public static async Task<IEnumerable<JObjectWrapper>> Wrap(Task<IEnumerable<JObject>> objects, string type) { return Wrap(await objects, type); }
        public static async Task<IEnumerable<JObjectWrapper>> Wrap(Task<IEnumerable<JObject>> objects, SfObjectBase type, string referenceId) { return Wrap(await objects, type, referenceId); }
        public static async Task<IEnumerable<JObjectWrapper>> Wrap(Task<IEnumerable<JObject>> objects, string type, string referenceId) { return Wrap(await objects, type, referenceId); }
        public static IAsyncEnumerable<JObjectWrapper> Wrap(IAsyncEnumerable<JObject> objects) { return objects?.Select(o => new JObjectWrapper(o)); }
        public static IAsyncEnumerable<JObjectWrapper> Wrap(IAsyncEnumerable<JObject> objects, SfObjectBase type) { return objects?.Select(o => new JObjectWrapper(o, type)); }
        public static IAsyncEnumerable<JObjectWrapper> Wrap(IAsyncEnumerable<JObject> objects, string type) { return objects?.Select(o => new JObjectWrapper(o, type)); }
        public static IAsyncEnumerable<JObjectWrapper> Wrap(IAsyncEnumerable<JObject> objects, SfObjectBase type, string referenceId) { return objects?.Select(o => new JObjectWrapper(o, type, referenceId)); }
        public static IAsyncEnumerable<JObjectWrapper> Wrap(IAsyncEnumerable<JObject> objects, string type, string referenceId) { return objects?.Select(o => new JObjectWrapper(o, type, referenceId)); }
        public static IEnumerable<JObjectWrapper> Wrap(IEnumerable<JObject> objects) { return objects?.Select(o => new JObjectWrapper(o)); }
        public static IEnumerable<JObjectWrapper> Wrap(IEnumerable<JObject> objects, SfObjectBase type) { return objects?.Select(o => new JObjectWrapper(o, type)); }
        public static IEnumerable<JObjectWrapper> Wrap(IEnumerable<JObject> objects, string type) { return objects?.Select(o => new JObjectWrapper(o, type)); }
        public static IEnumerable<JObjectWrapper> Wrap(IEnumerable<JObject> objects, SfObjectBase type, string referenceId) { return objects?.Select(o => new JObjectWrapper(o, type, referenceId)); }
        public static IEnumerable<JObjectWrapper> Wrap(IEnumerable<JObject> objects, string type, string referenceId) { return objects?.Select(o => new JObjectWrapper(o, type, referenceId)); }
    }
}
