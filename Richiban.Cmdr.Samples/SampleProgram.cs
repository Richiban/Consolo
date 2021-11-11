using System;

namespace Richiban.Cmdr.Samples
{
    [Cmdr("remote")]
    public class SampleProgram
    {
        [Cmdr("a")]
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
}