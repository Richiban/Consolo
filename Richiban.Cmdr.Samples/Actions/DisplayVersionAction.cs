using System;

namespace Richiban.Cmdr.Samples
{
    public static class DisplayVersionAction
    {
        [CommandLine, Route("version")]
        public static void Execute()
        {
            Console.WriteLine("Displaying version");
        }
    }
}