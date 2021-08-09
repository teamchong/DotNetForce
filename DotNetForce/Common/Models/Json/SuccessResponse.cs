using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace DotNetForce.Common.Models.Json
{
    public class SuccessResponse
    {
        [JsonProperty(PropertyName = "errors")]
        public object? Errors { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; }

        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }
        
        
        public static bool IsAssignableFrom(JToken token) =>
            token.Type == JTokenType.Object &&
            token["id"]?.Type == JTokenType.String &&
            token["success"]?.Type == JTokenType.Boolean &&
            token["errors"]?.Type == JTokenType.Array;

        public static SuccessResponse? TryCast(JToken token) =>
            IsAssignableFrom(token) ?
                new SuccessResponse
                {
                    Errors = token["Errors"]!,
                    Id = (string?)token["totalSize"] ?? string.Empty,
                    Success= (bool)token["done"]!
                } :
                null;

        public SuccessResponse Assert()
        {
            var errors = Errors == null ? JValue.CreateNull() : JToken.FromObject(Errors);
            if (!errors.Any()) return this;
            var messages = errors.Select(err => err?.ToString() ?? "Unknown Error.");
            throw new ForceException(Error.Unknown, string.Join(Environment.NewLine, messages));
        }
    }
}
