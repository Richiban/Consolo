using System;

namespace Richiban.Cmdr
{
    public class CmdrMethod : System.Attribute
    {
        public CmdrMethod(params string[] aliases)
        {
            Aliases = aliases;
        }

        public string[] Aliases { get; }
    }
}

namespace Richiban.Cmdr.Samples
{
    public static class SampleProgram
    {
        [CmdrMethod("test")]
        public static void UpdateStats(DateTime since, bool allowClobber)
        {
            Console.WriteLine(
                $"Updating stats since {since}, with {new { allowClobber }}");
        }
    }

    // Methods can be arbitrarily nested inside namespaces and static classes
    namespace InnerNamespace
    {
        public static class ContainerClass
        {
            [CmdrMethod("test1")]
            public static class InnerContainerClass
            {
                [CmdrMethod("test2")]
                public static void AnotherMethod(Data data)
                {
                    Console.WriteLine($"In {nameof(AnotherMethod)}, {new { data }}");
                }
            }

            // Method arguments don't have to be strings; they can be any type that has 
            // a constructor that takes a string
            public class Data
            {
                public Data(string value)
                {
                    Value = value;
                }

                public string Value { get; }

                public override string ToString() => Value;
            }
        }
    }
}