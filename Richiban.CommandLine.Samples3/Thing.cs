using System;
using Richiban.Cmdr.Generator;

namespace Richiban.CommandLine.Samples3
{
    public static class Thing
    {
        [CmdrMethod]
        public static void SomeFunction(string someArgument, bool someFlag)
        {
            Console.WriteLine(new { someArgument, someFlag });
        }
    }
}