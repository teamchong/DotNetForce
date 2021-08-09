using System;
using System.Net;
// ReSharper disable UnusedMember.Global

namespace DotNetForce.Common.Internals
{
    internal sealed class BaseHttpClientException : Exception
    {
        private readonly HttpStatusCode _httpStatusCode;

        internal BaseHttpClientException(string response, HttpStatusCode statusCode) : base(response)
        {
            _httpStatusCode = statusCode;
        }

        internal HttpStatusCode GetStatus()
        {
            return _httpStatusCode;
        }
    }
}
