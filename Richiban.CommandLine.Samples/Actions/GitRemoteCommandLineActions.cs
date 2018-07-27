using Richiban.CommandLine;
using System;

namespace ConsoleApp1
{
    class GitRemoteCommandLineActions
    {
        [CommandLine, Verb("remote")]
        public void ListRemotes()
        {
            $"Listing remotes".Dump();
        }

        [CommandLine, VerbSequence("remote", "add")]
        public void AddRemote(string remoteName, Uri remoteUri)
        {
            $"Adding remote with {new { remoteName, remoteUri }}".Dump();
        }

        [CommandLine, VerbSequence("remote", "remove")]
        public void RemoveRemote(string remoteName)
        {
            $"Removing remote {remoteName}".Dump();
        }

        [CommandLine, VerbSequence("remote", "prune")]
        public void PruneRemote(string remoteName)
        {
            $"Pruning remote {remoteName}".Dump();
        }
    }
}
