using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce.Schema
{
    public class SfDateTimeField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfDateTimeField(string path) : base(path) { }
    }
}
