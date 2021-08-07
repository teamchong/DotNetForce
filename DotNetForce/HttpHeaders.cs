using System;
using System.Collections.Generic;

namespace DotNetForce
{
    [JetBrains.Annotations.PublicAPI]
    public class HttpHeaders : Dictionary<string, string>
    {
        public HttpHeaders AutoAssign(bool enable)
        {
            if (enable) Remove("Sforce-Auto-Assign");
            else Add("Sforce-Auto-Assign", "FALSE");
            return this;
        }

        public HttpHeaders CallOptions(string client, string defaultNamespace = null)
        {
            if (string.IsNullOrEmpty(client)) Remove("Sforce-Call-Options");
            else Add("Sforce-Call-Options", string.IsNullOrEmpty(defaultNamespace) ? $"client={client}" : $"client={client}, defaultNamespace={defaultNamespace}");
            return this;
        }

        //public HttpHeaders LimitInfo(string apiUsage, string perAppApiUsage = null)
        //{
        //    if (string.IsNullOrEmpty(apiUsage))
        //    {
        //        Remove("Sforce-Limit-Info");
        //    }
        //    else
        //    {
        //        if (string.IsNullOrEmpty(perAppApiUsage))
        //        {
        //            Add("Sforce-Limit-Info", $"api-usage={apiUsage}");
        //        }
        //        else
        //        {
        //            Add("Sforce-Limit-Info", $"api-usage={apiUsage}, per-app-api-usage={perAppApiUsage}");
        //        }
        //    }
        //    return this;
        //}

        public HttpHeaders PackageVersion(string packageNamespace, string version)
        {
            if (string.IsNullOrEmpty(version)) Remove($"x-sfdc-packageversion-{packageNamespace}");
            else Add($"x-sfdc-packageversion-{packageNamespace}", version);
            return this;
        }

        public HttpHeaders QueryOptions(int? batchSize)
        {
            if (batchSize == null)
            {
                Remove("Sforce-Query-Options");
            }
            else
            {
                if (batchSize < 200 || batchSize > 2000) throw new ArgumentOutOfRangeException(nameof(batchSize));
                Add("Sforce-Query-Options", $"batchSize={batchSize}");
            }
            return this;
        }
    }
}
