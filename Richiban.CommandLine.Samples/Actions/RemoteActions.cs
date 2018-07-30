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
            string name,
            string branch = null, 
            [ShortForm('d')] bool delete = false,
            [ShortForm('a')] bool auto = false)
        {
            if (branch != null)
            {
                $"Setting head for remote {name} to branch {branch}".Dump();
            }
            else if (delete)
            {
                $"Deleting head for remote {name}".Dump();
            }
            else if(auto)
            {
                $"Auto-setting head for remote {name}".Dump();
            }
            else
            {
                Console.WriteLine("You must supply either a branch name or one of the flags [delete] or [auto]");
            }
        }
    }
}
