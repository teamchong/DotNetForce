using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce.Schema
{
    public class SfIdField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfIdField(string path) : base(path) { }
    }
}
