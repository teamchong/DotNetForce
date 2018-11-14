using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce
{
    public class SfDateField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfDateField(string path) : base(path) { }
    }
}
