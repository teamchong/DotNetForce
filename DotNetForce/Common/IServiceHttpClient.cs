using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetForce.Common.Models.Json;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace DotNetForce.Common
{
    public interface IServiceHttpClient : IDisposable
    {
        Task<T?> HttpGetAsync<T>(string resourceName) where T : class;
        Task<T?> HttpGetRestApiAsync<T>(string apiName) where T : class;
        Task<IList<T>> HttpGetAsync<T>(string resourceName, string nodeName);
        Task<T?> HttpGetAsync<T>(Uri uri) where T : class;
        Task<T?> HttpPostRestApiAsync<T>(string apiName, object inputObject) where T : class;
        Task<T?> HttpPostAsync<T>(object inputObject, string resourceName) where T : class;
        Task<T?> HttpPostAsync<T>(object inputObject, Uri uri) where T : class;
        Task<SuccessResponse> HttpPatchAsync(object inputObject, string resourceName);
        Task<bool> HttpDeleteAsync(string resourceName);
        Task<T?> HttpBinaryDataPostAsync<T>(string resourceName, object inputObject, byte[] fileContents, string headerName, string fileName) where T : class;
    }
}
