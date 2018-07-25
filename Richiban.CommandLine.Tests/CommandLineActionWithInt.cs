using Richiban.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class CommandLineActionWithInt
    {
        [CommandLine, Verb("TestInt")]
        public void Execute(int someNumber)
        {
            Program.Output = $"Receiving some number: {someNumber}";
        }
    }
}
