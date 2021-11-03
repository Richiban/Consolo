using Richiban.Cmdr;
using System;

namespace Richiban.Cmdr.Samples
{
    [Route("branch")]
    public sealed class BranchActions
    {
        [CommandLine, Route("")]
        public void Execute()
        {
            Console.WriteLine($"Listing branches");
        }

        [CommandLine, Route("")]
        public void Execute(string branchName)
        {
            Console.WriteLine($"Creating branch {branchName}");
        }
    }
}
