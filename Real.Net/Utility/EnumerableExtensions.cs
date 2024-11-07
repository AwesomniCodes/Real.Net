using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;

namespace Awesomni.Codes.Real.Net.Utility
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
            return enumerable;
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
                yield return item;
            }
        }

        public static IEnumerable<T> Yield<T>(this T @object)
        {
            yield return @object;
        }
    }
}
