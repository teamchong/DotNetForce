using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce.Schema
{
    public class SfUrlField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfUrlField(string path) : base(path) { }
    }
}
