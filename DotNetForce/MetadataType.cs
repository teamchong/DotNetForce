using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce
{
    public class MetadataType
    {
        protected string Value { get; set; }

        public MetadataType(string value) { Value = value ?? ""; }

        public override string ToString()
        {
            return Value;
        }
    }
}
