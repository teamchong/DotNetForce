#nullable enable
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// ReSharper disable UnusedMember.Global

namespace DotNetForce.Common.Models.Json
{
    public class QueryResult<T>
    {
        [JsonProperty(PropertyName = "nextRecordsUrl")]
        public string? NextRecordsUrl { get; set; }

        [JsonProperty(PropertyName = "totalSize")]
        public int TotalSize { get; set; }

        [JsonProperty(PropertyName = "done")]
        public bool Done { get; set; }

        [JsonProperty(PropertyName = "records")]
        public IList<T>? Records { get; set; }
        
        public static bool IsAssignableFrom(JToken? token) =>
            token?.Type == JTokenType.Object &&
            token["totalSize"]?.Type == JTokenType.Integer &&
            token["done"]?.Type == JTokenType.Boolean &&
            token["records"]?.Type == JTokenType.Array;

        public static QueryResult<T>? TryCast(JToken token) =>
            IsAssignableFrom(token) ?
            new QueryResult<T>
            {
                NextRecordsUrl = (string?)token["nextRecordsUrl"] ?? string.Empty,
                TotalSize =(int)token["totalSize"]!,
                Done = (bool)token["done"]!,
                Records = token["records"]?.ToObject<IList<T>>() ?? new List<T>()
            } :
            null;
    }
}
