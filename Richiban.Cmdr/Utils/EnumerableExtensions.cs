using System;
using System.Collections.Generic;

namespace Richiban.Cmdr
{
    static class EnumerableExtensions
    {
        public static IEnumerable<T> Choose<T>(this IEnumerable<Option<T>> source) =>
            Option<T>.Choose(source);

        public static (TProp, IReadOnlyList<T>) AllByMax<T, TProp>(
            this IEnumerable<T> source,
            Func<T, TProp> propertySelector)
            where TProp : IComparable<TProp>
        {
            var max = default(TProp);
            var result = new List<T>();

            foreach(var item in source)
            {
                var current = propertySelector(item);
                var compare = current.CompareTo(max);

                if (compare == 0)
                {
                    result.Add(item);
                }
                else if (compare > 0)
                {
                    max = current;
                    result = new List<T>();
                    result.Add(item);
                }
            }

            return (max, result.AsReadOnly());
        }
    }
}