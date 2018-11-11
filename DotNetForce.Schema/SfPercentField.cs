using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce.Schema
{
    public class SfPercentField<T> : SfFieldBase<T> where T: SfObjectBase
    {
        public SfPercentField(string path) : base(path) { }
    }
}
