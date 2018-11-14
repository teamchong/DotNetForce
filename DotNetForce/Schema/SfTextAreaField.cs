using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce
{
    public class SfTextAreaField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfTextAreaField(string path) : base(path) { }
    }
}
