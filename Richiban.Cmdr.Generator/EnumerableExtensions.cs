using System;
using System.Collections.Generic;

namespace Richiban.Cmdr.Generator
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<U> Choose<T, U>(
            this IEnumerable<T> source,
            Func<T, U?> selector)
        {
            foreach (var item in source)
            {
                var chosen = selector(item);

                if (chosen is not null)
                {
                    yield return chosen;
                }
            }
        }
    }
}