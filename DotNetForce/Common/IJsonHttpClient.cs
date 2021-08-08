using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace DotNetForce.Common
{
    public interface IJsonHttpClient : IDisposable
    {
        // GET
        Task<T?> HttpGetAsync<T>(string resourceName) where T : class;
        Task<T?> HttpGetAsync<T>(Uri uri) where T : class;
        Task<IList<T>> HttpGetAsync<T>(string resourceName, string nodeName);
        Task<T?> HttpGetRestApiAsync<T>(string apiName) where T : class;

        // POST
        Task<T?> HttpPostAsync<T>(object? inputObject, string resourceName) where T : class;
        Task<T?> HttpPostAsync<T>(object? inputObject, Uri uri) where T : class;
        Task<T?> HttpPostRestApiAsync<T>(string apiName, object? inputObject) where T : class;
        Task<T?> HttpBinaryDataPostAsync<T>(string resourceName, object? inputObject, byte[] fileContents, string headerName, string fileName) where T : class;

        // PATCH
        Task<SuccessResponse> HttpPatchAsync(object inputObject, string resourceName);
        Task<SuccessResponse> HttpPatchAsync(object inputObject, Uri uri);
        Task<SuccessResponse> HttpPatchAsync(object inputObject, string resourceName, bool ignoreNull);
        Task<SuccessResponse> HttpPatchAsync(object inputObject, Uri uri, NullValueHandling nullValueHandling);

        // DELETE
        Task<bool> HttpDeleteAsync(string resourceName);
        Task<bool> HttpDeleteAsync(Uri uri);
    }
}
