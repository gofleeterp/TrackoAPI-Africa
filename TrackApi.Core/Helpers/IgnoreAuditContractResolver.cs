using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TrackoApi.Core.Helpers
{
    public class IgnoreAuditContractResolver<T>:DefaultContractResolver where T:Attribute
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            // list the properties to ignore
            var propertiesToIgnore = type.GetProperties()
                .Where(x => x.GetCustomAttributes().OfType<T>().Any());

            // Build the properties list
            var properties = base.CreateProperties(type, memberSerialization);

            // only serialize properties that are not ignored
            properties = properties
                .Where(p => propertiesToIgnore.All(info => info.Name != p.UnderlyingName))
                .ToList();

            return properties;
        }
    }
}
