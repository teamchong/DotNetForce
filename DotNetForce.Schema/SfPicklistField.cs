using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetForce.Schema
{
    public class SfPicklistField<T> : SfFieldBase<T> where T : SfObjectBase
    {
        public string DefaultValue { get; set; }
        public SfPicklistValue[] PicklistValues { get; set; }
        public SfPicklistField(string path, string defaultValue, SfPicklistValue[] picklistValues) : base(path)
        {
            DefaultValue = defaultValue;
            PicklistValues = picklistValues;
        }
    }
}
