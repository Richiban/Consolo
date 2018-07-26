using System.Collections.Generic;

namespace Richiban.CommandLine
{
    static class EnumerableExtensions
    {
        public static IEnumerable<T> Choose<T>(this IEnumerable<Option<T>> source) =>
            Option<T>.Choose(source);
    }
}