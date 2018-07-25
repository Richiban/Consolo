using Richiban.CommandLine;

namespace ConsoleApp1
{
    public sealed class CleanWorkingDirectoryCommandLineAction
    {
        [CommandLine, Verb]
        public void Clean(
            [ShortForm('d')] bool removeDirectories = false,
            [ShortForm('f')] bool force = false,
            [ShortForm('x')] bool ignoreIgnore = false)
        {
            $"Cleaning working directory ({new { removeDirectories, force, ignoreIgnore }})".Dump();
        }
    }
}
