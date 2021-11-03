using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    static class Prelude
    {
        public static OptionNone None { get; } = new OptionNone();

        public static Option<T> Some<T>(T value) => new Option<T>(value);

        public struct OptionNone { }

        public static IReadOnlyList<T> ListOf<T>(params T[] items) => items.ToList().AsReadOnly();
    }
}
