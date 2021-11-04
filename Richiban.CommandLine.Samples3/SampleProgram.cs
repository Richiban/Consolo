using System;
using Richiban.Cmdr;

namespace Richiban
{
    namespace CommandLine
    {
        namespace Samples3
        {
            public static class ContainerClass
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
        }
    }
}