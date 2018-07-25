using Richiban.CommandLine;

namespace ConsoleApp1
{
    public sealed class DisplayVersionCommandLineAction
    {
        [CommandLine, Verb("", "version")]
        public void Execute()
        {
            $"Displaying version".Dump();
        }
    }
}
