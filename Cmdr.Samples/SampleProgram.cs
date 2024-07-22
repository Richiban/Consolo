using System;
using System.IO;

namespace Cmdr.Samples;

/// <summary>
/// A collection of test commands
/// </summary>
[Cmdr("test")]
public class TestCommands
{
    /// <summary>
    /// A test command that takes a file as a parameter
    /// </summary>
    /// <param name="file"></param>
    [Cmdr("")]
    public static void TestMethod(FileInfo file)
    {
        Console.WriteLine($"You supplied: '{file.FullName}'");
    }
}

/// <summary>
/// Commands related to checking out files and branches
/// </summary>
[Cmdr("checkout")]
public class CheckoutCommands
{
    /// <summary>
    /// Checks out a branch or file
    /// </summary>
    /// <param name="branchNameOrFileName">The name of the branch or file</param>
    [Cmdr("")]
    public static void Checkout(string branchNameOrFileName)
    {
        Console.WriteLine($"Checking out {branchNameOrFileName}");
    }
}

/// <summary>
/// Commands related to branches
/// </summary>
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

    /// <summary>
    /// Deletes a branch.
    /// </summary>
    /// <param name="branchName">The name of the branch to delete</param>
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
    [Cmdr("")]
    public static void ListRemotes()
    {
        Console.WriteLine("Listing remotes");
    }

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

class MathsCommands
{
    /// <summary>
    /// Performs a mathematical operation on two numbers
    /// </summary>
    [Cmdr("")]
    public static void PerformOperation(Operation op, int x, int y)
    {
        switch (op)
        {
            case Operation.Add:
                Console.WriteLine($"{x} + {y} = {x + y}");
                break;
            case Operation.Subtract:
                Console.WriteLine($"{x} - {y} = {x - y}");
                break;
            case Operation.Multiply:
                Console.WriteLine($"{x} * {y} = {x * y}");
                break;
            case Operation.Divide:
                Console.WriteLine($"{x} / {y} = {x / y}");
                break;
        }
    }

    public enum Operation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
}