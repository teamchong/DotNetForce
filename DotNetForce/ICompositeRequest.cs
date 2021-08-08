using System.Collections.Generic;
// ReSharper disable UnusedMemberInSuper.Global

namespace DotNetForce
{
    public interface ICompositeRequest
    {
        string Prefix { get; set; }
        bool AllOrNone { get; set; }
        IList<CompositeSubRequest> CompositeRequests { get; set; }
    }
}
