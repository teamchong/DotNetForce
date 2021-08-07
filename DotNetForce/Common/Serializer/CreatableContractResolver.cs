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
            var propInfo = type.GetRuntimeProperty(property.PropertyName);

            if (propInfo != null)
            {
                var creatableAttr = propInfo.GetCustomAttribute(typeof(CreatableAttribute), false);
                isCreatable = creatableAttr == null || ((CreatableAttribute)creatableAttr).Creatable;
            }

            return isCreatable;
        }
    }
}
