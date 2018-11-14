using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce
{
    public class SfLocationField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfLocationField(string path) : base(path) { }
    }
}
