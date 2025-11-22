using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Baramjk.Framework.Extensions
{
    public static class CollectionsEx
    {
        public static bool IsEmptyOrNull<T>(this IEnumerable<T> collection)
        {
            return collection == null || collection.Any() == false;
        }

        public static bool HasItem<T>(this IEnumerable<T> collection)
        {
            return collection?.Any() == true;
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
        {
            return source == null ? defaultValue : source.FirstOrDefault();
        }
    }
}