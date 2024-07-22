namespace Consolo;

static class Prelude
{
    public static Option<T> Some<T>(T value) => new(value);
    public static OptionNone None { get; } = new();

    public struct OptionNone {}
}