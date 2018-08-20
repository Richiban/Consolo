using Richiban.CommandLine;

namespace Richiban.CommandLine.Samples
{
    public class PushAction
    {
        /// <summary>
        /// This is a comment for my push action
        /// </summary>
        /// <param name="branchName">This is a comment for the branch name parameter</param>
        /// <param name="setUpstreamBranch"></param>
        /// <param name="setUpstreamBranchTo"></param>
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
