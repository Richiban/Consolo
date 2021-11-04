using System;
using Richiban.Cmdr.Generator;

namespace Richiban.CommandLine.Samples3
{
    public static class SampleProgram
    {
        [CmdrMethod]
        public static void UpdateStats(DateTime since, bool someFlag)
        {
            Console.WriteLine($"Inside {nameof(UpdateStats)}");
            Console.WriteLine(new { since, someFlag });
        }
    }
}