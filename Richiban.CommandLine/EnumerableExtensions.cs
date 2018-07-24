using System.Collections.Generic;

namespace Richiban.CommandLine
{
    static class EnumerableExtensions
    {
        public static IEnumerable<T> Choose<T>(this IEnumerable<Option<T>> source)
        {
            foreach (var item in source)
                if (item.HasValue)
                    yield return item.Value;
        }
    }
}