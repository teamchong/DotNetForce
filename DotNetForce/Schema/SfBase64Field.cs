using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce
{
    public class SfBase64Field<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfBase64Field(string path) : base(path) { }
    }
}
