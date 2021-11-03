using System;

namespace Richiban.Cmdr.Samples
{
    public sealed class CleanWorkingDirectoryAction
    {
        /// <summary>
        ///     Cleans the working directory
        /// </summary>
        /// <param name="removeDirectories">Removes empty directories</param>
        /// <param name="force"></param>
        /// <param name="ignoreIgnore"></param>
        [CommandLine, Route]
        public void Clean(
            [ShortForm(firstShortForm: 'd', DisallowLongForm = true)]
            bool removeDirectories = false,
            [ShortForm(firstShortForm: 'f', DisallowLongForm = true)] bool force = false,
            [ShortForm(firstShortForm: 'x', DisallowLongForm = true)] bool ignoreIgnore =
                false)
        {
            Console.WriteLine(
                $"Cleaning working directory ({new { removeDirectories, force, ignoreIgnore }})");
        }
    }
}