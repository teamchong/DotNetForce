using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetForce
{
    public class SfChildRelationship<T, TChild> where T: SfObjectBase where TChild: SfObjectBase, new()
    {
        protected string _Path { get; set; }
        public SfChildRelationship(string path) => _Path = path;
        //public string _(Func<TChild, string> soqlGetter) => soqlGetter(new TChild());
        public string As(out TChild child)
        {
            child = new TChild();
            return _Path;
        }
        public override string ToString() => _Path;
    }
}
