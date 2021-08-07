using System;
using System.Collections.Generic;
using System.Net;
using DotNetForce.Common.Models.Json;

namespace DotNetForce
{
    [JetBrains.Annotations.PublicAPI]
    public class ForceException : AggregateException
    {
        public ForceException(string error, string description)
            : this(ParseError(error), description) { }

        public ForceException(string error, string description, string[] fields)
            : this(error, description)
        {
            Fields = fields;
        }

        public ForceException(Error error, string description, string[] fields)
            : this(error, description)
        {
            Fields = fields;
        }

        public ForceException(string error, string description, HttpStatusCode httpStatusCode)
            : this(ParseError(error), description)
        {
            HttpStatusCode = httpStatusCode;
        }

        public ForceException(Error error, string description)
            : base(description)
        {
            Error = error;
            Fields = Array.Empty<string>();
            HttpStatusCode = new HttpStatusCode();
        }

        public ForceException(IEnumerable<Exception> exceptions)
            : base(exceptions)
        {

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
