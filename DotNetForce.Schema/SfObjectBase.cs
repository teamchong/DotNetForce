using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce.Schema
{
    public abstract class SfObjectBase
    {
        protected string _Path { get; set; }
        public SfObjectBase(string path) => _Path = path;
        public string _<T>(Func<T, string> soqlGetter)  where T : SfObjectBase, new() => soqlGetter(new T());
    }
}
