using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce
{
    public class SfComplexValueField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfComplexValueField(string path) : base(path) { }
    }
}
