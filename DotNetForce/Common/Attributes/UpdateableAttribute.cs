using System;

namespace DotNetForce.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UpdateableAttribute : Attribute
    {
        public UpdateableAttribute(bool updateable)
        {
            Updateable = updateable;
        }

        public bool Updateable { get; }
    }
}
