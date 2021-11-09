using System;

namespace Richiban.Cmdr.Samples.InnerNamespace
{
    public static class ContainerClass
    {
        [CmdrMethod("cmd")]
        public static class InnerContainerClass
        {
            [CmdrMethod("subcmd")]
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