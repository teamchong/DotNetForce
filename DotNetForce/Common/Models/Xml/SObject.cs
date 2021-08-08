using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
// ReSharper disable UnusedType.Global

namespace DotNetForce.Common.Models.Xml
{
    public sealed class SObject : Dictionary<string, object>, IXmlSerializable
    {
        public XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteRaw("<sObject>");
            foreach (var (key, o) in this)
                if (o is IXmlSerializable value)
                {
                    writer.WriteRaw($"<{key}>");
                    value.WriteXml(writer);
                    writer.WriteRaw($"</{key}>");
                }
                else
                {
                    writer.WriteRaw(string.Format("<{0}>{1}</{0}>", key, o));
                }
            writer.WriteRaw("</sObject>");
        }
    }
}
