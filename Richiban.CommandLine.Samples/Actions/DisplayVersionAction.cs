using Richiban.CommandLine;

namespace Richiban.CommandLine.Samples
{
    public static class DisplayVersionAction
    {
        //[CommandLine]
        public static void Execute(
            [ParameterName("version")] bool displayVersion = false)
        {
            $"Displaying version".Dump();
        }
    }
}
