using System;
using System.Collections.Generic;

namespace EntityCache.Extensions
{
    public static class LinqExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null) throw new ArgumentException(nameof(enumerable));
            if (action == null) throw new ArgumentException(nameof(action));

            foreach (T element in enumerable)
            {
                action(element);
            }
        }
    }
}
