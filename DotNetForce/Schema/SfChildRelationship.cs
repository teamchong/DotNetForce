// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
namespace DotNetForce.Schema
{
    public class SfChildRelationship<TChild> where TChild : SfObjectBase, new()
    {
        public SfChildRelationship(string path)
        {
            _Path = path;
        }

        // ReSharper disable once InconsistentNaming
        protected string _Path { get; set; }

        //public string _(Func<TChild, string> soqlGetter) => soqlGetter(new TChild());
        public string As(out TChild child)
        {
            child = new TChild();
            return _Path;
        }

        public override string ToString()
        {
            return _Path;
        }
    }
}
