using Richiban.CommandLine;

namespace ConsoleApp1
{
    public class PushCommandLineAction
    {
        [CommandLine, Verb]
        public void Push(string branchName = "self", bool setUpstreamBranch = false)
        {
            $"Pushing branch {branchName}".Dump();
        }
    }
}
