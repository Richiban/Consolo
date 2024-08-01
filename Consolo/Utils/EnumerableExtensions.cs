using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Consolo;
internal static class EnumerableExtensions
{
    public static string
        StringJoin<T>(this IEnumerable<T> source, string separator) =>
        string.Join(separator, source);

    public static IEnumerable<T> WhereIsSome<T>(this IEnumerable<Option<T>> source)
    {
        foreach (var item in source)
        {
            if (item.IsSome(out var value))
            {
                yield return value;
            }
        }
    }

    public static bool Contains<T>(this ISet<T> source, Option<T> option) =>
        option.IsSome(out var value) && source.Contains(value);

    public static void AddRange<T>(
        this ISet<T> source,
        IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            source.Add(value);
        }
    }

    public static ResultWithDiagnostics<IReadOnlyCollection<TResult>>
         CollectResults<TResult>(
            this IEnumerable<ResultWithDiagnostics<Option<TResult>>> source)
            where TResult : class
    {
        var results = ImmutableArray.CreateBuilder<TResult>();
        var diagnostics = ImmutableArray.CreateBuilder<DiagnosticModel>();

        foreach (var result in source)
        {
            if (result.Result.IsSome(out var value))
            {
                results.Add(value);
            }

            diagnostics.AddRange(result.Diagnostics);
        }

        return new(results.ToImmutable(), diagnostics.ToImmutable());
    }

    public static ResultWithDiagnostics<IReadOnlyCollection<TResult>>
         CollectResults<TResult>(
            this IEnumerable<ResultWithDiagnostics<TResult>> source)
            where TResult : class
    {
        var results = ImmutableArray.CreateBuilder<TResult>();
        var diagnostics = ImmutableArray.CreateBuilder<DiagnosticModel>();

        foreach (var result in source)
        {
            results.Add(result.Result);

            diagnostics.AddRange(result.Diagnostics);
        }

        return new(results.ToImmutable(), diagnostics.ToImmutable());
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

    public static string StringJoin(this IEnumerable<string> source, string separator) =>
        string.Join(separator, source);

    public static TProp MaxOrDefault<T, TProp>(
        this IEnumerable<T> source,
        Func<T, TProp> selector,
        TProp defaultValue = default!)
        where TProp : IComparable<TProp>
    {
        var max = defaultValue;

        foreach (var item in source)
        {
            var value = selector(item);

            if (value.CompareTo(max) > 0)
            {
                max = value;
            }
        }

        return max;
    }

    public static IEnumerable<T> Truncate<T>(
        this IEnumerable<T> source,
        int count,
        T truncateValue)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        using var e = source.GetEnumerator();

        while (count-- > 0 && e.MoveNext())
        {
            yield return e.Current;
        }

        if (e.MoveNext())
        {
            yield return truncateValue;
        }
    }
}