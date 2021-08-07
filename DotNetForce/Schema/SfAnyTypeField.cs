namespace DotNetForce.Schema
{
    public class SfAnyTypeField<T> : SfFieldBase where T : SfObjectBase
    {
        public SfAnyTypeField(string path) : base(path) { }
    }
}
