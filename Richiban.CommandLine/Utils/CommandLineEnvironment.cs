using System;
using System.IO;

namespace Richiban.CommandLine
{
    static class CommandLineEnvironment
    {
        public static string FlagGlyph { get; } = GetBestGuessAtFlagGlyph();
        public static string ShortFormFlagGlyph { get; } = GetBestGuessAtShortFormFlagGlyph();
        public static string ParameterGlyph { get; } = GetBestGuessAtParameterGlyph();

        private static string GetBestGuessAtParameterGlyph()
        {
            if (IsUnix())
            {
                return "--";
            }
            else
            {
                return "/";
            }
        }

        private static string GetBestGuessAtFlagGlyph()
        {
            if (IsUnix())
            {
                return "--";
            }
            else
            {
                return "/";
            }
        }

        private static string GetBestGuessAtShortFormFlagGlyph()
        {
            if (IsUnix())
            {
                return "-";
            }
            else
            {
                return "/";
            }
        }

        private static bool IsUnix() => Path.PathSeparator == '/';
    }
}
