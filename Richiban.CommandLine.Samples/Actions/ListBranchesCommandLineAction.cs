using Richiban.CommandLine;

namespace ConsoleApp1
{
    public sealed class ListBranchesCommandLineAction
    {
        [CommandLine, Verb("branch")]
        public void Execute()
        {
            $"Listing branches".Dump();
        }
    }
}
