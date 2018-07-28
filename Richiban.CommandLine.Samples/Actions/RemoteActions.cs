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
        public void SetHeadToBranch(string name, string branch)
        {
            $"Setting head for remote {name}".Dump();
        }

        [CommandLine, Route("set-head"), Flag('d', "delete")]
        public void SetHeadAuto(string name)
        {
            $"Auto-setting head for remote {name}".Dump();
        }

        [CommandLine, Route("set-head"), Flag('a', "auto")]
        public void SetHeadDelete(string name)
        {
            $"Deleting head for remote {name}".Dump();
        }
    }
}
