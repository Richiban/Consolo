using System;
using Cmdr;

namespace Richiban.Cmdr.Samples
{
    [Cmdr("remote")]
    public class RemoteActions
    {
        [Cmdr("")]
        public static void ListRemotes()
        {
            Console.WriteLine("Listing remotes");
        }
            
        [Cmdr("remove")]
        public static void RemoveRemote(string remoteName)
        {
            Console.WriteLine(
                $"Removing remote '{remoteName}'");
        }
        
        [Cmdr("add")]
        public static void AddRemote(string remoteName, Uri url)
        {
            Console.WriteLine(
                $"Adding remote '{remoteName}' with URI: '{url}'");
        }
    }
    
    [Cmdr("branch")]
    public class BranchActions
    {
        [Cmdr("")]
        public static void ListBranches()
        {
            Console.WriteLine("Listing branches");
        }
        
        [Cmdr("")]
        public static void CreateBranches(string branchName)
        {
            Console.WriteLine($"Creating branch {branchName}");
        }
    }

    [Cmdr("checkout")]
    public class CheckoutActions
    {
        [Cmdr("")]
        public static void CheckoutBranch(string branchName)
        {
            Console.WriteLine($"Checking out branch {branchName}");
        }
    }
    
    public class Root
    {
        [Cmdr("")]
        public static void MainRoot()
        {
            Console.WriteLine("Welcome to the pseudo-Git application!");
        }
    }
}