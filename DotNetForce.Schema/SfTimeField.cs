using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce.Schema
{
    public class SfTimeField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfTimeField(string path) : base(path) { }
    }
}
