namespace DotNetForce.Schema
{
    public class SfIdField<T> : SfFieldBase where T : SfObjectBase
    {
        public SfIdField(string path) : base(path) { }
    }
}
