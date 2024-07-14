using System;
using Cmdr;

namespace Richiban.Cmdr.Samples;

[Cmdr("branch")]
class BranchActions
{
    [Cmdr("")]
    public static void ListBranches()
    {
        Console.WriteLine("Listing branches");
    }

    [Cmdr("")]
    public static void CreateBranch(string newName, bool force = false)
    {
        Console.WriteLine($"Creating branch {newName}");
    }
}

/// <summary>
/// Hmmm
/// </summary>
[Cmdr("checkout")]
class CheckoutActions
{
    /// <summary>
    /// Checkout a branch by name
    /// </summary>
    /// <param name="branchName">The name of the branch to check out</param>
    /// <param name="force">Allow checkout even of local changes will be overwritten</param>
    [Cmdr("")]
    public static void Checkout(string branchName, bool force = false)
    {
        if (force)
        {
            Console.WriteLine($"Checking out branch {branchName} forcefully");
            return;
        }

        Console.WriteLine($"Checking out branch {branchName}");
    }
}