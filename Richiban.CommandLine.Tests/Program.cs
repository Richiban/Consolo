using Richiban.CommandLine;
using System;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {
        public static void Main()
        {
            var allArgs =
                new[] {
                    new string[] {},
                    new [] {"version"},
                    new [] {"branch"},
                    new [] {"branch", "myBranchName"},
                    new [] {"clean", "-xfd"},
                };

            foreach (var args in allArgs)
            {
                $"".Dump();
                $"{String.Join(", ", args)} => ".Dump();
                CommandLine.Execute(args);
            }

            if (Debugger.IsAttached) Console.ReadLine();
        }
    }
}
