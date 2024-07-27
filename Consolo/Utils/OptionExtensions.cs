using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Collections.Immutable;

namespace Consolo;

static class OptionExtensions
{
    public static Option<string> Trim(this Option<string> opt) => 
        opt.IsSome(out var v) ? v.Trim() : null;

    public static ImmutableArray<T>.Enumerator GetEnumerator<T>(this Option<ImmutableArray<T>> opt) =>
        opt.IsSome(out var v) ? v.GetEnumerator() : ImmutableArray<T>.Empty.GetEnumerator();
}