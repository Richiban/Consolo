using Richiban.CommandLine;
using System;

namespace ConsoleApp1
{
    class RemoteActions
    {
        [CommandLine, Verb("remote")]
        public void ListRemotes()
        {
            $"Listing remotes".Dump();
        }

        [CommandLine, VerbSequence("remote", "add")]
        public void AddRemote(string name, Uri remoteUri)
        {
            $"Adding remote with {new { name, remoteUri }}".Dump();
        }

        [CommandLine, VerbSequence("remote", "remove")]
        public void RemoveRemote(string name)
        {
            $"Removing remote {name}".Dump();
        }

        [CommandLine, VerbSequence("remote", "prune")]
        public void PruneRemote(string name)
        {
            $"Pruning remote {name}".Dump();
        }

        [CommandLine, VerbSequence("remote", "set-head")]
        public void SetHeadToBranch(string name, string branch)
        {
            $"Setting head for remote {name}".Dump();
        }

        [CommandLine, VerbSequence("remote", "set-head")]
        public void SetHeadAuto(string name, [ShortForm('d')] bool delete)
        {
            $"Auto-setting head for remote {name}".Dump();
        }

        [CommandLine, VerbSequence("remote", "set-head")]
        public void SetHeadDelete(string name, [ShortForm('a')] bool auto)
        {
            $"Deleting head for remote {name}".Dump();
        }
    }
}
