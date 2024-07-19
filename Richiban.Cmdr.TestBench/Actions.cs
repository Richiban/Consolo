using System;
using System.IO;

namespace Richiban.Cmdr.Samples;

/// <summary>
/// A collection of actions for checking out branches and files
/// </summary>
class CheckoutActions
{
    /// <summary>
    /// Check out a branch
    /// </summary>
    /// <param name="branchName">The name of the branch to check out</param>
    /// <param name="force">Allow the checkout to overwrite local changes</param>
    public static void CheckoutBranch(
        string branchName,
        bool force = false)
    {
        Console.WriteLine(new {
            branchName,
            force
        });
    }
}