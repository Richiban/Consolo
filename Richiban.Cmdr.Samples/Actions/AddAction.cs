using System;
using System.IO;

namespace Richiban.Cmdr.Samples.Actions
{
    internal class AddAction
    {
        [CommandLine, Route]
        public void Add(params FileInfo[] files) =>
            Console.WriteLine($"Adding files: {string.Join(", ", (object[])files)}");
    }
}