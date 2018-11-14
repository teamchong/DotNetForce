using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce
{
    public class SfUrlField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfUrlField(string path) : base(path) { }
    }
}
