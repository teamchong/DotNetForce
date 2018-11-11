using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce.Schema
{
    public class SfDateField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfDateField(string path) : base(path) { }
    }
}
