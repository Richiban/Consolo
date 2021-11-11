using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Richiban.Cmdr.Utils
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<TResult> SelectNotNull<T, TResult>(
            this IEnumerable<T> source,
            Func<T, TResult?> selector)
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

        public static string
            StringJoin<T>(this IEnumerable<T> source, string separator) =>
            string.Join(separator, source);

        public static (ImmutableArray<TSuccess> Successes, ImmutableArray<TError> Failures
            ) SeparateResults<TSuccess, TError>(
                this IEnumerable<Result<TError, TSuccess>> source)
        {
            var successes = ImmutableArray.CreateBuilder<TSuccess>();
            var failures = ImmutableArray.CreateBuilder<TError>();

            foreach (var item in source)
            {
                switch (item)
                {
                    case Result<TError, TSuccess>.Ok(var success):
                        successes.Add(success);

                        break;

                    case Result<TError, TSuccess>.Error(var failure):
                        failures.Add(failure);

                        break;
                }
            }

            return (successes.ToImmutable(), failures.ToImmutable());
        }

        public static IEnumerable<T> Truncate<T>(
            this IReadOnlyCollection<T> source,
            int count)
        {
            var toTake = count switch
            {
                >= 0 => count,
                < 0 => source.Count + count
            };

            if (toTake == 0)
            {
                yield break;
            }

            using var e = source.GetEnumerator();

            while (toTake-- > 0)
            {
                e.MoveNext();

                yield return e.Current;
            }
        }
    }
}