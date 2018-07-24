using Richiban.CommandLine;

namespace ConsoleApp1
{
    [Verb("", "version")]
    public sealed class DisplayVersionCommandLineAction : ICommandLineAction
    {
        public void Execute()
        {
            $"Displaying version".Dump();
        }
    }
}
