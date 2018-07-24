using Richiban.CommandLine;

namespace ConsoleApp1
{
    [Verb("Branch")]
    public sealed class NewBranchCommandLineAction : ICommandLineAction
    {
        public string Name { get; set; }

        public void Execute()
        {
            $"Creating new branch ({Name})".Dump();
        }
    }
}
