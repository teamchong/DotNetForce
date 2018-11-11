using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce.Schema
{
    public abstract class SfFieldBase<T> where T: SfObjectBase
    {
        protected string _Path { get; set; }
        public SfFieldBase(string path) => _Path = path;
        public override string ToString() => _Path;
    }
}
