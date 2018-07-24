using Richiban.CommandLine;
using System;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {
        public static void Main(string[] args)
        {
            CommandLine.Execute(args);

            if (Debugger.IsAttached) Console.ReadLine();
        }
    }
}
