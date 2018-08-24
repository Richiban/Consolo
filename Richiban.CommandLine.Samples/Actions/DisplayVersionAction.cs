using Richiban.CommandLine;
using System;

namespace Richiban.CommandLine.Samples
{
    public static class DisplayVersionAction
    {
        [CommandLine, Route("version")]
        public static void Execute()
        {
            Console.WriteLine($"Displaying version");
        }
    }
}
