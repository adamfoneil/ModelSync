using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelSync.Library.Extensions
{
    public static class OrderHelper
    {
        /// <summary>
        /// orders a list of T so that root items are first followed by its dependencies
        /// </summary>
        public static HashSet<T> ToDependencyOrder<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> getDependencies)
        {
            HashSet<T> results = new HashSet<T>();

            var rootItems = items.Where(item => !getDependencies(item).Any());
            var remainingItems = items.Except(rootItems);

            foreach (var item in rootItems) results.Add(item);

            foreach (var item in remainingItems)
            {
                var deps = getDependencies(item).ToArray();
                if (results.IsSupersetOf(deps))
                {
                    results.Add(item);
                }
            }

            return results;
        }
    }
}
