using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cmdr;

internal static class NonEmptyList
{
    public static NonEmptyList<T> Create<T>(IEnumerable<T> items)
    {
        var any = false;
        var first = default(T);

        IEnumerable<T> wrap()
        {
            foreach (var item in items)
            {
                if (!any)
                {
                    first = item;
                }

                any = true;

                yield return item;
            }
        }

        var wrapped = wrap();

        if (!any)
        {
            throw new EmptySequenceException(
                $"Cannot create a {nameof(NonEmptyList<T>)} from an empty sequence");
        }

        return new NonEmptyList<T>(first!, wrapped.Skip(count: 1).ToArray());
    }

    private class EmptySequenceException : Exception
    {
        public EmptySequenceException(string message) : base(message)
        {
        }
    }
}

internal class NonEmptyList<T> : IReadOnlyList<T>
{
    private readonly T _first;
    private readonly T[] _subsequent;

    public NonEmptyList(T first, params T[] subsequent)
    {
        _first = first;
        _subsequent = subsequent;
    }

    public IEnumerator<T> GetEnumerator()
    {
        yield return First();

        foreach (var item in _subsequent)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _subsequent.Length + 1;

    public T this[int index] =>
        index switch
        {
            1 => First(),
            var i => _subsequent[i - 1]
        };

    /// <summary>
    ///     Gets the first item from the non-empty list
    /// </summary>
    /// <returns></returns>
    public T First() => _first;

    public static implicit operator NonEmptyList<T>(T item) => new(item);
}