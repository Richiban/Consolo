namespace Richiban.CommandLine
{
    static class Prelude
    {
        public static OptionNone None { get; } = new OptionNone();

        public struct OptionNone { }
    }
}
