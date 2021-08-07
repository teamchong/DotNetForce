using System.Collections.Generic;
using System.Xml.Serialization;

namespace DotNetForce.Common.Models.Xml
{
    [XmlRoot(ElementName = "results",
        Namespace = "http://www.force.com/2009/06/asyncapi/dataload")]
    public class BatchResultList
    {
        [XmlElement("result")] public IList<BatchResult> Items;

        public BatchResultList()
        {
            Items = new List<BatchResult>();
        }
    }
}
