using System;
using System.ComponentModel;
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
        
        [Cmdr("add", Description = "Adds a remote to the repository")]
        public static void AddRemote(
            string remoteName,
            Uri url,
            [Cmdr("overwrite", Description = "Allow overwriting existing")] bool allowClobber = false)
        {
            Console.WriteLine(
                $"Adding remote '{remoteName}' with URI: '{url}'. Allow clobber: {allowClobber}");
        }
    }
    
    [Cmdr("branch", Description = "Branch management commands")]
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

    [Cmdr("log")]
    public class LogActions
    {
        [Cmdr("")]
        public static void ShowLog(int logItemCount = 1)
        {
            Console.WriteLine($"Showing {logItemCount} log items");
        }
    }
    
    public class Root
    {
        [Cmdr("")]
        public static void MainRoot()
        {
            Console.WriteLine("Welcome to the pseudo-Git application!");
        }

        [Cmdr("test", Description = "This is a test command")]
        public static void Test(bool flag)
        {
            Console.WriteLine($"This is a test command: {flag}");
        }
    }
}