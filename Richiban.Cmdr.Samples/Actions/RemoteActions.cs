using Richiban.Cmdr;
using System;

namespace Richiban.Cmdr.Samples
{
    [Route("remote")]
    class RemoteActions
    {
        [CommandLine, Route("")]
        public void ListRemotes()
        {
            Console.WriteLine($"Listing remotes");
        }

        /// <summary>
        /// Add a new remote
        /// </summary>
        /// <param name="name">The name to give the new remote</param>
        /// <param name="remoteUri">The URI of the new remote</param>
        [CommandLine, Route("add")]
        public void AddRemote(string name, Uri remoteUri)
        {
            Console.WriteLine($"Adding remote with {new { name, remoteUri }}");
        }

        [CommandLine, Route("remove")]
        public void RemoveRemote(string name)
        {
            Console.WriteLine($"Removing remote {name}");
        }

        [CommandLine, Route("prune")]
        public void PruneRemote(string name)
        {
            Console.WriteLine($"Pruning remote {name}");
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
                Console.WriteLine($"Setting head for remote {remoteName} to branch {branch}");
            }
            else if (deleteRemote)
            {
                Console.WriteLine($"Deleting head for remote {remoteName}");
            }
            else if(automaticallySetHead)
            {
                Console.WriteLine($"Auto-setting head for remote {remoteName}");
            }
            else
            {
                Console.WriteLine("You must supply either a branch name or one of the flags [delete] or [auto]");
            }
        }
    }
}
