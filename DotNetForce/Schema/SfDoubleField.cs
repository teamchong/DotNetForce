using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce
{
    public class SfDoubleField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfDoubleField(string path) : base(path) { }
    }
}
