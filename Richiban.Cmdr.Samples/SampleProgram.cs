using System;

namespace Richiban.Cmdr.Samples
{
    [Cmdr("remote")]
    public class SampleProgram
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
    }

    public class Root
    {
        [Cmdr("")]
        public static void MainRoot()
        {
            Console.WriteLine("In the root command");
        }
    }
}