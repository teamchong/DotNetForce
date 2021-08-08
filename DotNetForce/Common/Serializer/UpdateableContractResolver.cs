using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetForce.Common.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DotNetForce.Common.Serializer
{
    public class UpdateableContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization)
                .Where(p => IsPropertyUpdateable(type, p))
                .ToList();
        }

        private static bool IsPropertyUpdateable(Type type, JsonProperty property)
        {
            var isUpdateable = true;
            if (property.PropertyName == null) return isUpdateable;
            var propInfo = type.GetRuntimeProperty(property.PropertyName);

            if (propInfo == null) return isUpdateable;
            var updateableAttr = propInfo.GetCustomAttribute(typeof(UpdateableAttribute), false);
            isUpdateable = updateableAttr == null || ((UpdateableAttribute)updateableAttr).Updateable;

            return isUpdateable;
        }
    }
}
