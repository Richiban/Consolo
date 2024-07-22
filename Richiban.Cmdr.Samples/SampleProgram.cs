using System;
using Cmdr;

namespace Richiban.Cmdr.Samples;

[Cmdr("checkout")]
public class CheckoutCommands
{
    [Cmdr("")]
    public static void Checkout(string branchNameOrFileName, Foo thing)
    {
        Console.WriteLine($"Checking out {branchNameOrFileName}");
    }
}

public class Foo 
{
    // cast from string
    public static implicit operator Foo(string value) => new Foo {  };
}

[Cmdr("branch")]
public class BranchCommands
{
    /// <summary>
    /// Lists all the branches.
    /// </summary>
    [Cmdr("")]
    public static void ListBranches()
    {
        Console.WriteLine("Listing branches");
    }

    /// <summary>
    /// Creates a new branch.
    /// </summary>
    /// <param name="branchName">The name of the new branch</param>
    [Cmdr("create")]
    public static void CreateBranch(string branchName)
    {
        Console.WriteLine($"Creating branch {branchName}");
    }

    [Cmdr("delete")]
    public static void DeleteBranch(string branchName)
    {
        Console.WriteLine($"Deleting branch {branchName}");
    }
}

/// <summary>
/// Actions for adding and removing remotes
/// </summary>
[Cmdr("remote")]
public class RemoteActions
{
    /// <summary>
    /// Lists the remotes in the repository
    /// </summary>
    // [Cmdr("")]
    // public static void ListRemotes()
    // {
    //     Console.WriteLine("Listing remotes");
    // }

    /// <summary>
    /// Adds a remote to the repository
    /// </summary>
    /// <param name="name">The local name to give the remote</param>
    /// <param name="url">The URL of the remote</param>
    /// <param name="force">Overwrite any existing remote with the given name</param>
    [Cmdr("add")]
    public static void AddRemote(
        [Cmdr("long-name")] string name,
        Uri url,
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