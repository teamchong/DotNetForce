namespace DotNetForce.Schema
{
    public class SfComboBoxField<T> : SfFieldBase where T : SfObjectBase
    {
        public SfComboBoxField(string path, string defaultValue, SfPicklistValue[] picklistValues) : base(path)
        {
            DefaultValue = defaultValue;
            PicklistValues = picklistValues;
        }

        public string DefaultValue { get; set; }
        public SfPicklistValue[] PicklistValues { get; set; }
    }
}
