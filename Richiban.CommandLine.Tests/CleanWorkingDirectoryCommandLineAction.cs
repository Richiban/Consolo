using Richiban.CommandLine;

namespace ConsoleApp1
{
    [Verb("clean")]
    public sealed class CleanWorkingDirectoryCommandLineAction : ICommandLineAction
    {
        [Switch, AlternativeName("d")]
        public bool RemoveDirectories { get; set; }
        [Switch, AlternativeName("f")]
        public bool Force { get; set; }
        [Switch, AlternativeName("x")]
        public bool IgnoreIgnore { get; set; }

        public void Execute()
        {
            $"Cleaning working directory ({new { RemoveDirectories, Force, IgnoreIgnore }})".Dump();
        }
    }
}
