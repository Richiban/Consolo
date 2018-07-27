using System.Collections.Generic;

namespace Richiban.CommandLine
{
    static class EnumerableExtensions
    {
        public static IEnumerable<T> Choose<T>(this IEnumerable<Option<T>> source) =>
            Option<T>.Choose(source);

        public static IEnumerable<(T1, T2)> Zip<T1, T2>(this IEnumerable<T1> sequence1, IEnumerable<T2> sequence2)
        {
            var e1 = sequence1.GetEnumerator();
            var e2 = sequence2.GetEnumerator();

            while(e1.MoveNext() && e2.MoveNext())
            {
                yield return (e1.Current, e2.Current);
            }
        }
    }
}