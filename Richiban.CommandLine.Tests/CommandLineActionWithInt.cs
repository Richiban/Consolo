using Richiban.CommandLine;

namespace ConsoleApp1
{
    class CommandLineActionWithInt
    {
        [CommandLine, Verb("TestInt")]
        public void Execute(int someNumber)
        {
            $"Receiving some number: {someNumber}".Dump();
        }
    }
}
