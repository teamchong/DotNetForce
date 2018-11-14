using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json.Linq;

namespace DotNetForce
{
    public class SfAddressField<T> : SfFieldBase<T> where T : SfObjectBase
    {
        public SfAddressField(string path) : base(path) { }
    }
}
