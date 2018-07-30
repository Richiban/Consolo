using System;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    static class EnumerableExtensions
    {
        public static IEnumerable<T> Choose<T>(this IEnumerable<Option<T>> source) =>
            Option<T>.Choose(source);

        public static IEnumerable<T> MaxByAll<T, TProp>(
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

            return result.AsReadOnly();
        }

        public static (IReadOnlyCollection<T>, IReadOnlyCollection<T>) Partition<T>(
            this IEnumerable<T> source,
            Func<T, bool> partitionBy)
        {
            var trueList = new List<T>();
            var falseList = new List<T>();

            foreach(var item in source)
            {
                if(partitionBy(item))
                {
                    trueList.Add(item);
                }
                else
                {
                    falseList.Add(item);
                }
            }

            return (trueList, falseList);
        }
    }
}