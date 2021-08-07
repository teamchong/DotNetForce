namespace DotNetForce.Schema
{
    public class SfMultiPicklistField : SfFieldBase
    {
        public SfMultiPicklistField(string path, string defaultValue, SfPicklistValue[] picklistValues) : base(path)
        {
            DefaultValue = defaultValue;
            PicklistValues = picklistValues;
        }

        public string DefaultValue { get; set; }
        public SfPicklistValue[] PicklistValues { get; set; }
    }
}
