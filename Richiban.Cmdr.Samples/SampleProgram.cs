using System;
using Cmdr;

namespace Richiban.Cmdr.Samples;

/// <summary>
/// A collection of actions for adding and removing remotes
/// </summary>
[Cmdr("remote")]
public class RemoteActions
{
    /// <summary>
    /// Adds a remote to the repository
    /// </summary>
    /// <param name="name">The local name to give the remote</param>
    /// <param name="url">The URL of the remote</param>
    /// <param name="force">Overwrite any existing remote with the given name</param>
    [Cmdr("add")]
    public static void AddRemote(
        [Cmdr("long-name")] string name,
        string url,
        [Cmdr(ShortForm = "f")] bool force = false)
    {
        if (force)
        {
            Console.WriteLine($"Adding remote {name} at {url} (force)");
            return;
        }

        Console.WriteLine($"Adding remote {name} at {url}");
    }

    /// <summary>
    /// Removes a remote from the repository
    /// </summary>
    /// <param name="name">The local name of the remote</param>
    [Cmdr("remove")]
    public static void RemoveRemote(string name)
    {
        Console.WriteLine($"Removing remote {name}");
    }
}