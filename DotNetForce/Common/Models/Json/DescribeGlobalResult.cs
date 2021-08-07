using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce.Common.Models.Json
{
    public class DescribeGlobalResult<T>
    {
        [JsonProperty(PropertyName = "encoding")]
        public string Encoding { get; set; }

        [JsonProperty(PropertyName = "maxBatchSize")]
        public int MaxBatchSize { get; set; }

        [JsonProperty(PropertyName = "sobjects")]
        public IList<T> SObjects { get; set; }
    }
}
