using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce.Schema
{
    public class SfStringField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfStringField(string path) : base(path) { }
    }
}
