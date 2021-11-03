using System;

namespace Richiban.Cmdr.Samples
{
    public class PushAction
    {
        /// <summary>
        ///     This is a comment for my push action
        /// </summary>
        /// <param name="branchName">The branch to push</param>
        /// <param name="setUpstreamBranch">Add an upstream tracking reference to the default remote branch</param>
        /// <param name="setUpstreamBranchTo">Add an upstream tracking reference to the given remote branch</param>
        [CommandLine, Route]
        public void Push(
            string branchName = null,
            [ShortForm(firstShortForm: 'u')] bool setUpstreamBranch = false,
            string setUpstreamBranchTo = null)
        {
            Console.WriteLine(
                $"Pushing branch {new { branchName, setUpstreamBranch, setUpstreamBranchTo }}");
        }
    }
}