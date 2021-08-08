using System.Collections.Generic;
// ReSharper disable UnusedMemberInSuper.Global

namespace DotNetForce
{
    public interface IBatchRequest
    {
        string Prefix { get; set; }
        bool HaltOnError { get; set; }
        IList<BatchSubRequest> BatchRequests { get; set; }
    }
}
