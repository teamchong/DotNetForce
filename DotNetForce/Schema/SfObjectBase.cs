using System;

namespace DotNetForce.Schema
{
    public abstract class SfObjectBase
    {
        protected SfObjectBase(string path)
        {
            _Path = path;
        }

        // ReSharper disable once InconsistentNaming
        protected string _Path { get; set; }

        public string _<T>(Func<T, string> soqlGetter) where T : SfObjectBase, new()
        {
            return soqlGetter(new T());
        }
    }
}
