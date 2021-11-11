using System;

namespace Richiban.Cmdr.Samples
{
    [CmdrMethod("update")]
    public class SampleProgram
    {
        [CmdrMethod("stats")]
        public static void UpdateStats(DateTime since, bool allowClobber)
        {
            Console.WriteLine(
                $"Updating stats since {since}, with {new { allowClobber }}");
        }
    }
}