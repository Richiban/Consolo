using System.IO;

namespace Richiban.CommandLine
{
    static class CommandLineEnvironment
    {
        public static string FlagGlyph { get; } = GetBestGuessAtFlagGlyph();
        public static string ShortFormFlagGlyph { get; } = GetBestGuessAtShortFormFlagGlyph();

        internal static string GetBestGuessAtFlagGlyph()
        {
            if (Path.PathSeparator == '/')
            {
                return "--";
            }
            else
            {
                return "/";
            }
        }

        internal static string GetBestGuessAtShortFormFlagGlyph()
        {
            if (Path.PathSeparator == '/')
            {
                return "-";
            }
            else
            {
                return "/";
            }
        }
    }
}
