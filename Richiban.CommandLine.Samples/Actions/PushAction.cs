using Richiban.CommandLine;

namespace Richiban.CommandLine.Samples
{
    public class PushAction
    {
        [CommandLine, Route]
        public void Push(
            string branchName = null,
            [ShortForm('u')] bool setUpstreamBranch = false,
            string setUpstreamBranchTo = null)
        {
            $"Pushing branch {new { branchName, setUpstreamBranch, setUpstreamBranchTo }}".Dump();
        }
    }
}
