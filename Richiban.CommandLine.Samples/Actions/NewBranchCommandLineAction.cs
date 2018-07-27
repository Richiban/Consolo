using Richiban.CommandLine;

namespace ConsoleApp1
{
    public sealed class NewBranchCommandLineAction
    {
        [CommandLine, Verb("branch")]
        public void Execute(string branchName)
        {
            $"Creating branch {branchName}".Dump();
        }
    }
}
