using Richiban.CommandLine;
using System;
using System.Diagnostics;

namespace ConsoleApp1
{
    /// <summary>
    /// This program is intended to be a copy of the git command line.
    /// 
    /// None of the actions actually do anything (obviously), just print out which one was called and
    /// with which arguments so that you can see how the CommandLine attributes work.
    /// 
    /// Git was chosen as a model because it has an extensive command line interface with many commands
    /// and queries that can be executed. It's a complicated domain to try and model which means that, if
    /// we can do Git, we should be able to do anything :)
    /// </summary>
    class Program
    {
        public static void Main(string[] args)
        {
            {
                CommandLine.Run(args);
                return;
            }

            var tests = new[] {
                (new [] { "branch", "/?" }, "Listing branches"),
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
            CommandLine.Run(args);
        }

        internal static string Output { get; set; }
    }
}
