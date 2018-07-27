using Richiban.CommandLine;

namespace ConsoleApp1
{
    public class PushCommandLineAction
    {
        [CommandLine, Verb]
        public void Push(
            string branchName = null,
            [ShortForm('u')] bool setUpstreamBranch = false,
            string setUpstreamBranchTo = null)
        {
            $"Pushing branch {new { branchName, setUpstreamBranch, setUpstreamBranchTo }}".Dump();
        }
    }
}
