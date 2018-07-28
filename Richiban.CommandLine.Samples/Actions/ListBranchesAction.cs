using Richiban.CommandLine;

namespace Richiban.CommandLine.Samples
{
    public sealed class ListBranchesAction
    {
        [CommandLine, Route("branch")]
        public void Execute()
        {
            $"Listing branches".Dump();
        }
    }
}
