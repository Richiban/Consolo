using System;
using Richiban.Cmdr.Generator;

namespace Richiban.CommandLine.Samples3
{
    public static class SampleProgram
    {
        [CmdrMethod]
        public static void UpdateStats(DateTime since, bool allowClobber)
        {
            Console.WriteLine(
                $"Updating stats since {since}, with {new { allowClobber }}");
        }
    }
}