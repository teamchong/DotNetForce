using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace DotNetForce.Schema
{
    public class JObjectWrapper : IAttributedObject
    {
        public JObjectWrapper() { Object = new JObject(); }

        public JObjectWrapper(SfObjectBase? type) : this(new JObject(), type?.ToString()) { }

        public JObjectWrapper(SfObjectBase? type, string? referenceId) : this(new JObject(), type?.ToString(), referenceId) { }

        public JObjectWrapper(string? type) : this(new JObject(), type) { }

        public JObjectWrapper(string? type, string? referenceId) : this(new JObject(), type, referenceId) { }

        public JObjectWrapper(JObject? obj) { Object = obj ?? new JObject(); }

        public JObjectWrapper(JObject? obj, SfObjectBase? type) : this(obj, type?.ToString()) { }

        public JObjectWrapper(JObject? obj, string? type)
        {
            if (obj != null) obj["attributes"] = new JObject { ["type"] = type };
            Object = obj ?? new JObject();
        }

        public JObjectWrapper(JObject? obj, SfObjectBase? type, string? referenceId) : this(obj, type?.ToString(), referenceId) { }

        public JObjectWrapper(JObject? obj, string? type, string? referenceId)
        {
            if (obj != null) obj["attributes"] = new JObject { ["type"] = type, ["referenceId"] = referenceId };
            Object = obj ?? new JObject();
        }

        [JsonExtensionData]
        protected JObject Object { get; set; }

        [JsonIgnore]
        public ObjectAttributes Attributes {
            get => Object["attributes"]?.ToObject<ObjectAttributes>() ?? new ObjectAttributes();
            set => Object["attributes"] = JObject.FromObject(value);
        }

        public JObjectWrapper Spread(string sep = ":")
        {
            var result = new JObject();
            foreach (var prop in Object.Properties())
            {
                var splits = prop.Name.Split(new[] { sep }, 2, StringSplitOptions.None);
                if (splits.Length == 1)
                {
                    result[prop.Name] = prop.Value;
                }
                else if (result[prop.Name]?.Type == JTokenType.Object)
                {
                    var subObj = result[prop.Name] ?? new JObject();
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

        public JObject Unwrap(SfObjectBase? type)
        {
            return Unwrap(type?.ToString());
        }

        public JObject Unwrap(string? type)
        {
            Object["attributes"] = new JObject { ["type"] = type };
            return Object;
        }

        public JObject Unwrap(SfObjectBase? type, string? referenceId)
        {
            return Unwrap(type?.ToString(), referenceId);
        }

        public JObject Unwrap(string? type, string? referenceId)
        {
            Object["attributes"] = new JObject { ["type"] = type, ["referenceId"] = referenceId };
            return Object;
        }

        public JToken Get(string? path)
        {
            var paths = path?.Split(new[] { '.' }, 2);
            if (paths?.Length == 1) return Object[paths[0]] ?? JValue.CreateNull();
            if (!(paths?.Length > 1)) return JValue.CreateNull();
            return Object[paths[0]]?.Type == JTokenType.Object ? new JObjectWrapper((JObject?)Object[paths[0]]).Get(paths[1]) : JValue.CreateNull();
        }

        public JObjectWrapper Set(string? path, JToken? value)
        {
            var paths = path?.Split(new[] { '.' }, 2) ?? Array.Empty<string>();
            if (paths.Length == 1)
            {
                Object[paths[0]] = value;
            }
            else if (paths.Length > 1)
            {
                if (Object[paths[0]]?.Type != JTokenType.Object) Object[paths[0]] = new JObject();
                new JObjectWrapper((JObject?)Object[paths[0]]).Set(paths[1], value);
            }
            return this;
        }


        public JToken Get(SfAddressField? field)
        {
            return Get(field?.ToString());
        }

        public JObjectWrapper Set(SfAddressField? field, JObject? value)
        {
            return Set(field?.ToString(), value);
        }


        public JToken Get(SfAnyTypeField? field)
        {
            return Get(field?.ToString());
        }

        public JObjectWrapper Set(SfAnyTypeField? field, JObject? value)
        {
            return Set(field?.ToString(), value);
        }


        public string Get(SfBase64Field? field)
        {
            return Get(field?.ToString()).ToString();
        }

        public JObjectWrapper Set(SfBase64Field? field, string? value)
        {
            return Set(field?.ToString(), value);
        }


        public bool? Get(SfBooleanField? field)
        {
            return (bool?)Get(field?.ToString());
        }

        public JObjectWrapper Set(SfBooleanField? field, bool? value)
        {
            return Set(field?.ToString(), value);
        }


        public QueryResult<JObject> Get<TChild>(SfChildRelationship<TChild>? field) where TChild : SfObjectBase, new()
        {
            return Get(field?.ToString()).ToObject<QueryResult<JObject>>() ?? new QueryResult<JObject>();
        }

        public JObjectWrapper Set<TChild>(SfChildRelationship<TChild>? field, JObject? value) where TChild : SfObjectBase, new()
        {
            return Set(field?.ToString(), value);
        }


        public string Get(SfComboBoxField? field)
        {
            return Get(field?.ToString()).ToString();
        }

        public JObjectWrapper Set(SfComboBoxField? field, string? value)
        {
            return Set(field?.ToString(), value);
        }


        public JToken Get(SfComplexValueField? field)
        {
            return Get(field?.ToString());
        }

        public JObjectWrapper Set(SfComplexValueField? field, JObject? value)
        {
            return Set(field?.ToString(), value);
        }


        public double? Get(SfCurrencyField? field)
        {
            return (double?)Get(field?.ToString());
        }

        public JObjectWrapper Set(SfCurrencyField? field, double? value)
        {
            return Set(field?.ToString(), value);
        }

        public JObjectWrapper Set(SfCurrencyField? field, decimal? value)
        {
            return Set(field?.ToString(), value);
        }


        public DateTime? Get(SfDateField? field)
        {
            return Dnf.ToDateTime(Get(field?.ToString()).ToString());
        }

        public JObjectWrapper Set(SfDateField? field, DateTime? value)
        {
            return Set(field?.ToString(), Dnf.SoqlDate(value));
        }


        public DateTime? Get(SfDateTimeField? field)
        {
            return Dnf.ToDateTime(Get(field?.ToString()).ToString());
        }

        public JObjectWrapper Set(SfDateTimeField? field, DateTime? value)
        {
            return Set(field?.ToString(), Dnf.SoqlDateTime(value));
        }


        public double? Get(SfDoubleField? field)
        {
            return (double?)Get(field?.ToString());
        }

        public JObjectWrapper Set(SfDoubleField? field, double? value)
        {
            return Set(field?.ToString(), value);
        }

        public JObjectWrapper Set(SfDoubleField? field, decimal? value)
        {
            return Set(field?.ToString(), value);
        }


        public string Get(SfEmailField? field)
        {
            return Get(field?.ToString()).ToString();
        }

        public JObjectWrapper Set(SfEmailField? field, string? value)
        {
            return Set(field?.ToString(), value);
        }


        public string Get(SfIdField? field)
        {
            return Dnf.ToId18(Get(field?.ToString()).ToString());
        }

        public JObjectWrapper Set(SfIdField? field, string? value)
        {
            return Set(field?.ToString(), Dnf.ToId15(value));
        }


        public long? Get(SfIntField? field)
        {
            return (long?)Get(field?.ToString());
        }

        public JObjectWrapper Set(SfIntField? field, long? value)
        {
            return Set(field?.ToString(), value);
        }


        public JToken Get(SfLocationField? field)
        {
            return Get(field?.ToString());
        }

        public JObjectWrapper Set(SfLocationField? field, JObject? value)
        {
            return Set(field?.ToString(), value);
        }


        public string Get(SfMultiPicklistField? field)
        {
            return Get(field?.ToString()).ToString();
        }

        public JObjectWrapper Set(SfMultiPicklistField? field, string? value)
        {
            return Set(field?.ToString(), value);
        }


        public double? Get(SfPercentField? field)
        {
            return (double?)Get(field?.ToString());
        }

        public JObjectWrapper Set(SfPercentField? field, double? value)
        {
            return Set(field?.ToString(), value);
        }

        public JObjectWrapper Set(SfPercentField? field, decimal? value)
        {
            return Set(field?.ToString(), value);
        }


        public string Get(SfPhoneField? field)
        {
            return Get(field?.ToString()).ToString();
        }

        public JObjectWrapper Set(SfPhoneField? field, string? value)
        {
            return Set(field?.ToString(), value);
        }


        public string Get(SfPicklistField? field)
        {
            return Get(field?.ToString()).ToString();
        }

        public JObjectWrapper Set(SfPicklistField? field, string? value)
        {
            return Set(field?.ToString(), value);
        }


        public string Get(SfStringField? field)
        {
            return Get(field?.ToString()).ToString();
        }

        public JObjectWrapper Set(SfStringField? field, string? value)
        {
            return Set(field?.ToString(), value);
        }


        public string Get(SfTextAreaField? field)
        {
            return Get(field?.ToString()).ToString();
        }

        public JObjectWrapper Set(SfTextAreaField? field, string? value)
        {
            return Set(field?.ToString(), value);
        }


        public DateTime? Get(SfTimeField? field)
        {
            return Dnf.FromSoqlTime(Get(field?.ToString()).ToString());
        }

        public JObjectWrapper Set(SfTimeField? field, DateTime? value)
        {
            return Set(field?.ToString(), Dnf.SoqlTime(value));
        }


        public string Get(SfUrlField? field)
        {
            return Get(field?.ToString()).ToString();
        }

        public JObjectWrapper Set(SfUrlField? field, string? value)
        {
            return Set(field?.ToString(), value);
        }

        public override string ToString()
        {
            return Object.ToString();
        }

        public string ToString(Formatting formatting, params JsonConverter[] converters)
        {
            return Object.ToString(formatting, converters);
        }

        public static implicit operator JObjectWrapper(JObject? obj)
        {
            return new JObjectWrapper(obj);
        }

        public static implicit operator JObject(JObjectWrapper? wrapper)
        {
            return wrapper?.Unwrap() ?? new JObject();
        }
        public static JObjectWrapper Wrap(JObject? obj) =>
            new JObjectWrapper(obj);
        public static JObjectWrapper Wrap(JObject? obj, SfObjectBase? type) =>
            new JObjectWrapper(obj, type);
        public static JObjectWrapper Wrap(JObject? obj, string? type) =>
            new JObjectWrapper(obj, type);
        public static JObjectWrapper Wrap(JObject? obj, SfObjectBase? type, string? referenceId) =>
            new JObjectWrapper(obj, type, referenceId);
        public static JObjectWrapper Wrap(JObject? obj, string? type, string? referenceId) =>
            new JObjectWrapper(obj, type, referenceId);
        public static async Task<IEnumerable<JObjectWrapper>> Wrap(Task<IEnumerable<JObject>> objects) =>
            Wrap(await objects
                .ConfigureAwait(false));
        public static async Task<IEnumerable<JObjectWrapper>> Wrap(Task<IEnumerable<JObject>> objects, SfObjectBase? type) =>
            Wrap(await objects
                .ConfigureAwait(false), type);
        public static async Task<IEnumerable<JObjectWrapper>> Wrap(Task<IEnumerable<JObject>> objects, string? type) =>
            Wrap(await objects
                .ConfigureAwait(false), type);
        public static async Task<IEnumerable<JObjectWrapper>> Wrap(Task<IEnumerable<JObject>> objects, SfObjectBase? type, string? referenceId) =>
            Wrap(await objects
                .ConfigureAwait(false), type, referenceId);

        public static async Task<IEnumerable<JObjectWrapper>> Wrap(Task<IEnumerable<JObject>> objects, string? type, string? referenceId) =>
            Wrap(await objects
                .ConfigureAwait(false), type, referenceId);

        public static async IAsyncEnumerable<JObjectWrapper> Wrap(IAsyncEnumerable<JObject>? objects)
        {
            if (objects == null) yield break;
            await foreach (var obj in objects
                .ConfigureAwait(false))
                yield return new JObjectWrapper(obj);
        }

        public static async IAsyncEnumerable<JObjectWrapper> Wrap(IAsyncEnumerable<JObject>? objects, SfObjectBase? type)
        {
            if (objects == null) yield break;
            await foreach (var obj in objects
                .ConfigureAwait(false))
                yield return new JObjectWrapper(obj, type);
        }

        public static async IAsyncEnumerable<JObjectWrapper> Wrap(IAsyncEnumerable<JObject>? objects, string? type)
        {
            if (objects == null) yield break;
            await foreach (var obj in objects
                .ConfigureAwait(false))
                yield return new JObjectWrapper(obj, type);
        }

        public static async IAsyncEnumerable<JObjectWrapper> Wrap(IAsyncEnumerable<JObject>? objects, SfObjectBase? type, string? referenceId)
        {
            if (objects == null) yield break;
            await foreach (var obj in objects
                .ConfigureAwait(false))
                yield return new JObjectWrapper(obj, type, referenceId);
        }

        public static async IAsyncEnumerable<JObjectWrapper> Wrap(IAsyncEnumerable<JObject>? objects, string? type, string? referenceId)
        {
            if (objects == null) yield break;
            await foreach (var obj in objects
                .ConfigureAwait(false))
                yield return new JObjectWrapper(obj, type, referenceId);
        }
        public static IEnumerable<JObjectWrapper> Wrap(IEnumerable<JObject>? objects) => objects?.Select(o =>
            new JObjectWrapper(o)) ?? Enumerable.Empty<JObjectWrapper>();
        public static IEnumerable<JObjectWrapper> Wrap(IEnumerable<JObject>? objects, SfObjectBase? type) => objects?.Select(o =>
            new JObjectWrapper(o, type)) ?? Enumerable.Empty<JObjectWrapper>();
        public static IEnumerable<JObjectWrapper> Wrap(IEnumerable<JObject>? objects, string? type) => objects?.Select(o =>
            new JObjectWrapper(o, type)) ?? Enumerable.Empty<JObjectWrapper>();
        public static IEnumerable<JObjectWrapper> Wrap(IEnumerable<JObject>? objects, SfObjectBase? type, string? referenceId) =>
            objects?.Select(o => new JObjectWrapper(o, type, referenceId)) ?? Enumerable.Empty<JObjectWrapper>();
        public static IEnumerable<JObjectWrapper> Wrap(IEnumerable<JObject>? objects, string? type, string? referenceId) =>
            objects?.Select(o => new JObjectWrapper(o, type, referenceId)) ?? Enumerable.Empty<JObjectWrapper>();
    }
}
