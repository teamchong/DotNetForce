using Microsoft.JSInterop;
using Microsoft.AspNetCore.Blazor.Services;
using System;
using System.Collections.Generic;

namespace BlazorForce
{
    public static class IUriHelperExtension
    {
        public static string GetHash(this IUriHelper uri)
        {
            var uriSplit = uri.GetAbsoluteUri().Split(new[] { '#' }, 2);

            if (uriSplit.Length > 1)
            {
                return uriSplit[1];
            }
            return "";
        }

        public static string GetHash(this IUriHelper uri, string path)
        {
            var hash = GetHash(uri);
            if (hash.StartsWith(path))
            {
                return hash.Substring(path.Length);
            }
            return null;
        }

        public static IDictionary<string, string> GetHashAsDictionary(this IUriHelper uri, string path)
        {
            var hashVal = GetHash(uri, path);
            if (hashVal != null)
            {
                var hashSplit = hashVal.Split(new[] { '&' });
                var output = new Dictionary<string, string>();

                foreach (var hashItem in hashSplit)
                {
                    var keyValSplit = hashItem.Split(new[] { '=' });
                    var key = Uri.UnescapeDataString(keyValSplit[0]);
                    var val = keyValSplit.Length > 1 ? Uri.UnescapeDataString(keyValSplit[1]) : "";
                    if (!output.ContainsKey(key))
                    {
                        output.Add(key, val);
                    }
                }
                return output;
            }
            return null;
        }
    }
}