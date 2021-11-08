using System;

namespace Richiban.Cmdr.Samples
{
    public class SampleProgram
    {
        [CmdrMethod("test")]
        public static void UpdateStats(DateTime since, bool allowClobber)
        {
            Console.WriteLine(
                $"Updating stats since {since}, with {new { allowClobber }}");
        }
    }
}