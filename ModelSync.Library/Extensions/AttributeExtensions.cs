using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModelSync.Extensions
{
    public static class AttributeExtensions
    {
        public static T GetAttribute<T>(this ICustomAttributeProvider provider) where T : Attribute
        {
            try
            {
                var attrs = provider.GetCustomAttributes(typeof(T), true).OfType<T>();
                return (attrs?.Any() ?? false) ? attrs.FirstOrDefault() : null;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public static bool HasAttribute<T>(this ICustomAttributeProvider provider, out T attribute) where T : Attribute
        {
            attribute = GetAttribute<T>(provider);
            return (attribute != null);
        }

        public static T GetAssemblyAttribute<T>(this System.Reflection.Assembly assembly) where T : Attribute
        {
            var attribs = assembly.GetCustomAttributes(typeof(T), false);
            if (attribs == null || attribs.Length == 0)
                return null;
            else
                return attribs.OfType<T>().SingleOrDefault();
        }

        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }
}
