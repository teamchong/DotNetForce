using System;
using System.Threading.Tasks;
// ReSharper disable UnusedMemberInSuper.Global

namespace DotNetForce.Common
{
    public interface IXmlHttpClient : IDisposable
    {
        // GET
        Task<T?> HttpGetAsync<T>(string resourceName) where T : class;
        Task<T?> HttpGetAsync<T>(Uri uri) where T : class;

        // POST
        Task<T?> HttpPostAsync<T>(object? inputObject, string resourceName) where T: class;
        Task<T?> HttpPostAsync<T>(object? inputObject, Uri uri) where T: class;
    }
}
