// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
namespace DotNetForce.Schema
{
    public abstract class SfFieldBase
    {
        protected SfFieldBase(string path)
        {
            _Path = path;
        }

        // ReSharper disable once InconsistentNaming
        protected string _Path { get; set; }

        public override string ToString()
        {
            return _Path;
        }
    }
}
