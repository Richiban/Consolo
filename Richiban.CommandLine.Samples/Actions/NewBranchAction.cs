using Richiban.CommandLine;

namespace Richiban.CommandLine.Samples
{
    public sealed class NewBranchAction
    {
        [CommandLine, Verb("branch")]
        public void Execute(string branchName)
        {
            $"Creating branch {branchName}".Dump();
        }
    }
}
