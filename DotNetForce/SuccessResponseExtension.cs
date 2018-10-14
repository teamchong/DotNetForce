using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using System;
using System.Linq;

namespace DotNetForce
{
    public static class SuccessResponseExtension
    {
        public static void ThrowIfError(this SuccessResponse response)
        {
            if (response?.Errors != null)
            {
                try
                {
                    var errors = JArray.FromObject(response.Errors);
                    if (errors.Count > 0)
                    {
                        var messages = errors.Select(err => err?.ToString() ?? "Unknown Error.");
                        throw new ForceException(Error.Unknown, string.Join(Environment.NewLine, messages));
                    }
                }
                catch { }
            }
        }
    }
}

