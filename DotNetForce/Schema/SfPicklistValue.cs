using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce
{
    public class SfPicklistValue
    {
        public SfPicklistValue(string value) : this(value, value) { }
        public SfPicklistValue(string value, string label) { Value = value; Label = label; }

        public string Value { get; set; }
        public string Label { get; set; }

        public override string ToString() => Value;
    }
}
