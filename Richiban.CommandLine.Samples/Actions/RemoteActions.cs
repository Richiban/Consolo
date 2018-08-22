using Richiban.CommandLine;
using System;

namespace Richiban.CommandLine.Samples
{
    [Route("remote")]
    class RemoteActions
    {
        [CommandLine, Route("")]
        public void ListRemotes()
        {
            $"Listing remotes".Dump();
        }

        /// <summary>
        /// Add a new remote
        /// </summary>
        /// <param name="name">The name to give the new remote</param>
        /// <param name="remoteUri">The URI of the new remote</param>
        [CommandLine, Route("add")]
        public void AddRemote(string name, Uri remoteUri)
        {
            $"Adding remote with {new { name, remoteUri }}".Dump();
        }

        [CommandLine, Route("remove")]
        public void RemoveRemote(string name)
        {
            $"Removing remote {name}".Dump();
        }

        [CommandLine, Route("prune")]
        public void PruneRemote(string name)
        {
            $"Pruning remote {name}".Dump();
        }

        [CommandLine, Route("set-head")]
        public void SetHeadToBranch(
            [ParameterName("name")] string remoteName,
            string branch = null, 
            [ShortForm('d'), ParameterName("delete")] bool deleteRemote = false,
            [ShortForm('a'), ParameterName("auto")] bool automaticallySetHead = false)
        {
            if (branch != null)
            {
                $"Setting head for remote {remoteName} to branch {branch}".Dump();
            }
            else if (deleteRemote)
            {
                $"Deleting head for remote {remoteName}".Dump();
            }
            else if(automaticallySetHead)
            {
                $"Auto-setting head for remote {remoteName}".Dump();
            }
            else
            {
                Console.WriteLine("You must supply either a branch name or one of the flags [delete] or [auto]");
            }
        }
    }
}
