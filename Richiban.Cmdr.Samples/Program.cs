using Richiban.CommandLine;
using System;
using System.Diagnostics;

namespace Richiban.CommandLine.Samples
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
        public static void Main(string[] args) => CommandLine.Execute(args);
    }
}
