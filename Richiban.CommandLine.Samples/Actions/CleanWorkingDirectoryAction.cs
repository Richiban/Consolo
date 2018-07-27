using Richiban.CommandLine;

namespace Richiban.CommandLine.Samples
{
    public sealed class CleanWorkingDirectoryAction
    {
        [CommandLine, Verb]
        public void Clean(
            [ShortForm('d', DisallowLongForm = true)] bool removeDirectories = false,
            [ShortForm('f', DisallowLongForm = true)] bool force = false,
            [ShortForm('x', DisallowLongForm = true)] bool ignoreIgnore = false)
        {
            $"Cleaning working directory ({new { removeDirectories, force, ignoreIgnore }})".Dump();
        }
    }
}
