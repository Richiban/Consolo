using Richiban.CommandLine;

namespace ConsoleApp1
{
    [Verb("branch")]
    public sealed class ListBranchesCommandLineAction : ICommandLineAction
    {
        public void Execute()
        {
            $"Listing branches".Dump();
        }
    }
}
