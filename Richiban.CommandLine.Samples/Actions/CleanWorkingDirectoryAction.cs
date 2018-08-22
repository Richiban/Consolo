using Richiban.CommandLine;
using System;

namespace Richiban.CommandLine.Samples
{
    public sealed class CleanWorkingDirectoryAction
    {
        /// <summary>
        /// Cleans the working directory
        /// </summary>
        /// <param name="removeDirectories">Removes empty directories</param>
        /// <param name="force"></param>
        /// <param name="ignoreIgnore"></param>
        [CommandLine, Route]
        public void Clean(
            [ShortForm('d', DisallowLongForm = true)] bool removeDirectories = false,
            [ShortForm('f', DisallowLongForm = true)] bool force = false,
            [ShortForm('x', DisallowLongForm = true)] bool ignoreIgnore = false)
        {
            $"Cleaning working directory ({new { removeDirectories, force, ignoreIgnore }})".Dump();
        }
    }
}
