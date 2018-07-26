using Richiban.CommandLine;
using System;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {
        public static void Main(string[] args)
        {
            var tests = new[] {
                (new [] { "/?" }, "Help"),
                (new string[0], "Displaying version"),
                (new [] { "version" }, "Displaying version"),
                (new [] { "branch" }, "Listing branches"),
                (new [] { "branch", "myNewBranchName" }, "Creating branch myNewBranchName"),
                (new [] { "branch", "/branchName:myNewBranchName2" }, "Creating branch myNewBranchName2"),
                (new [] { "branch", "-branchName", "myNewBranchName3" }, "Creating branch myNewBranchName3"),
                (new [] { "clean" }, "Cleaning working directory ({ removeDirectories = False, force = False, ignoreIgnore = False })"),
                (new [] { "clean", "-x" }, "Cleaning working directory ({ removeDirectories = False, force = False, ignoreIgnore = True })"),
                (new [] { "clean", "-xfd" }, "Cleaning working directory ({ removeDirectories = True, force = True, ignoreIgnore = True })"),
                (new [] { "clean", "/x", "/f" }, "Cleaning working directory ({ removeDirectories = False, force = True, ignoreIgnore = True })"),
                (new [] { "clean", "/remove", "/f" }, "Cleaning working directory ({ removeDirectories = True, force = True, ignoreIgnore = False })"),
                (new [] { "testint", "-somenumber", "99" }, "Receiving some number: 99"),
            };

            foreach (var (input, output) in tests)
                RunTest(input, output);

            if (Debugger.IsAttached) Console.ReadLine();
        }

        private static void RunTest(string[] args, string expectedOutput)
        {
            Console.WriteLine(String.Join(" ", args));
            CommandLine.Execute(args);
        }

        internal static string Output { get; set; }
    }
}
