using System;
using System.Linq;
using System.Reflection;

namespace ModelSync.Extensions
{
    public static class AttributeExtensions
    {
        public static T GetAttribute<T>(this ICustomAttributeProvider provider) where T : Attribute
        {
            var attrs = provider.GetCustomAttributes(typeof(T), true).OfType<T>();
            return (attrs?.Any() ?? false) ? attrs.FirstOrDefault() : null;
        }

        public static bool HasAttribute<T>(this ICustomAttributeProvider provider, out T attribute) where T : Attribute
        {
            attribute = GetAttribute<T>(provider);
            return (attribute != null);
        }
    }
}
