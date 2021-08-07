using System;

namespace DotNetForce.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CreatableAttribute : Attribute
    {
        public CreatableAttribute(bool creatable)
        {
            Creatable = creatable;
        }

        public bool Creatable { get; }
    }
}
