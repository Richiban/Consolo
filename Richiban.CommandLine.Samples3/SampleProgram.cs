using System;
using Richiban.Cmdr;

namespace Richiban.Cmdr
{
    namespace Samples3
    {
        public static class ContainerClass
        {
            public static class SampleProgram
            {
                [CmdrMethod]
                public static void UpdateStat(DateTime since, bool allowClobber)
                {
                    Console.WriteLine(
                        $"Updating stats since {since}, with {new { allowClobber }}");
                }
            }
        }
    }
}

namespace SomethingElse
{
    public static class C
    {
        [CmdrMethod]
        public static void M(Data data)
        {
            Console.WriteLine($"In M, {new { data }}");
        }
    }

    public class Data
    {
        public Data(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}