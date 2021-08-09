// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
namespace DotNetForce.Schema
{
    public class SfPicklistValue
    {
        public SfPicklistValue(string value) : this(value, value) { }

        public SfPicklistValue(string value, string label)
        {
            Value = value;
            Label = label;
        }

        public string Value { get; set; }
        public string Label { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}
