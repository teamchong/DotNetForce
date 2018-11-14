using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetForce
{
    public class SfComboBoxField<T> : SfFieldBase<T> where T : SfObjectBase
    {
        public string DefaultValue { get; set; }
        public SfPicklistValue[] PicklistValues { get; set; }
        public SfComboBoxField(string path, string defaultValue, SfPicklistValue[] picklistValues) : base(path)
        {
            DefaultValue = defaultValue;
            PicklistValues = picklistValues;
        }
    }
}
