using System;
using Cmdr;

namespace Richiban.Cmdr.Samples;

[Cmdr("branch")]
class BranchActions
{
    /// <summary>
    /// Lists branches
    /// </summary>
    [Cmdr("")]
    public static void ListBranches()
    {
        Console.WriteLine("Listing branches");
    }

    /// <summary>
    /// Creates a new branch
    /// </summary>
    /// <param name="newName">The name of the new branch</param>
    /// <param name="force">Whether to overwrite any existing branch with the same name</param>
    [Cmdr("")]
    public static void CreateBranch(string newName, bool force = false)
    {
        Console.WriteLine($"Creating branch {newName}");
    }
}

[Cmdr("remote")]
class RemoteActions
{
    /// <summary>
    /// Lists remotes
    /// </summary>
    [Cmdr("")]
    public static void ListRemotes()
    {
        Console.WriteLine("Listing remotes");
    }

    /// <summary>
    /// Adds a remote
    /// </summary>
    /// <param name="remoteName">The name of the remote</param>
    /// <param name="remoteUrl">The URL of the remote</param>
    [Cmdr("add")]
    public static void AddRemote(string remoteName, string remoteUrl)
    {
        Console.WriteLine($"Adding remote {remoteName} at {remoteUrl}");
    }
}

[Cmdr("checkout")]
class CheckoutActions
{
    /// <summary>
    /// Check out a branch
    /// </summary>
    /// <param name="branchName">The name of the branch to check out</param>
    /// <param name="force">Allow overwriting local changes with checked out files</param>
    /// <param name="remote">Check out a remote branch</param>
    [Cmdr("")]
    public static void CheckoutBranch(
        string branchName, 
        [Cmdr(ShortForm = "r")] bool remote = false,
        [Cmdr(ShortForm = "f")] bool force = false)
    {
        Console.WriteLine(
            (force, remote) switch {
                (true, true) => $"Forcefully checking out *remote* branch {branchName}",
                (true, false) => $"Forcefully checking out branch {branchName}",
                (false, true) => $"Checking out *remote* branch {branchName}",
                (false, false) => $"Checking out branch {branchName}"
            }
        );
    }
}