using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce.Schema
{
    public class SfEmailField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfEmailField(string path) : base(path) { }
    }
}
