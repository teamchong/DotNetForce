// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
namespace DotNetForce
{
    public class MetadataType
    {
        public MetadataType(string value) { Value = value; }
        protected string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}
