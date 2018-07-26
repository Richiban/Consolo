using Richiban.CommandLine;

namespace ConsoleApp1
{
    public static class DisplayVersionCommandLineAction
    {
        [CommandLine, Verb("", "version")]
        public static void Execute()
        {
            $"Displaying version".Dump();
        }
    }
}
