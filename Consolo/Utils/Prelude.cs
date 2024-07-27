global using static Consolo.Prelude;

namespace Consolo;

static class Prelude
{
    public static Option<T> Some<T>(T value) => new(value);
    public static OptionNone None { get; } = new();
    public static Option<T> Maybe<T>(this T value) => new(value);

    public struct OptionNone {}
}