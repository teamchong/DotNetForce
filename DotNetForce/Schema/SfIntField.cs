using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce
{
    public class SfIntField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfIntField(string path) : base(path) { }
    }
}
