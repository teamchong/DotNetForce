using System;
using System.Net;
using DotNetForce.Common.Models.Json;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace DotNetForce
{
    public class ForceAuthException : Exception
    {
        public ForceAuthException(string error, string description)
            : this(ParseError(error), description) { }

        public ForceAuthException(string error, string description, string[] fields)
            : this(error, description)
        {
            Fields = fields;
        }

        public ForceAuthException(Error error, string description, string[] fields)
            : this(error, description)
        {
            Fields = fields;
        }

        public ForceAuthException(string error, string description, HttpStatusCode httpStatusCode)
            : this(ParseError(error), description)
        {
            HttpStatusCode = httpStatusCode;
        }

        public ForceAuthException(Error error, string description)
            : base(description)
        {
            Error = error;
            Fields = Array.Empty<string>();
            HttpStatusCode = new HttpStatusCode();
        }

        public string[] Fields { get; }
        public HttpStatusCode HttpStatusCode { get; }
        public Error Error { get; }

        private static Error ParseError(string error)
        {
            return Enum.TryParse(error.Replace("_", ""), true, out Error value) ? value : Error.Unknown;
        }
    }
}
