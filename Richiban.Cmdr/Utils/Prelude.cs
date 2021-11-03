using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.Cmdr
{
    internal static class Prelude
    {
        public static OptionNone None { get; } = new();

        public static Option<T> Some<T>(T value) => new(value);

        public static IReadOnlyList<T> ListOf<T>(params T[] items) =>
            items.ToList().AsReadOnly();

        public struct OptionNone
        {
        }
    }
}