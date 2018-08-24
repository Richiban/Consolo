using Richiban.CommandLine;
using System;

namespace Richiban.CommandLine.Samples
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
