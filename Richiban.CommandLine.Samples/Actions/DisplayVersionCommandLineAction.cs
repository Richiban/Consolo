using Richiban.CommandLine;

namespace ConsoleApp1
{
    public static class DisplayVersionCommandLineAction
    {
        //[CommandLine]
        public static void Execute(
            [ParameterName("version")] bool displayVersion = false)
        {
            $"Displaying version".Dump();
        }
    }
}
