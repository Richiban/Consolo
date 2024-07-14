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

[Cmdr("remote")]
class RemoteActions
{
    [Cmdr("")]
    public static void ListRemotes()
    {
        Console.WriteLine("Listing remotes");
    }

    [Cmdr("add")]
    public static void AddRemote(string remoteName, string remoteUrl)
    {
        Console.WriteLine($"Adding remote {remoteName} at {remoteUrl}");
    }
}