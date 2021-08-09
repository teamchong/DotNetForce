using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetForce.Common.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DotNetForce.Common.Serializer
{
    public class CreatableContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization)
                .Where(p => IsPropertyCreatable(type, p))
                .ToList();
        }

        private static bool IsPropertyCreatable(Type type, JsonProperty property)
        {
            var isCreatable = true;
            if (property.PropertyName == null) return isCreatable;
            var propInfo = type.GetRuntimeProperty(property.PropertyName);

            if (propInfo == null) return isCreatable;
            var creatableAttr = propInfo.GetCustomAttribute(typeof(CreatableAttribute), false);
            isCreatable = creatableAttr == null || ((CreatableAttribute)creatableAttr).Creatable;

            return isCreatable;
        }
    }
}
