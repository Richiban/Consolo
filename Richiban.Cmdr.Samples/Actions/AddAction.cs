using System;
using System.IO;

namespace Richiban.Cmdr.Samples.Actions
{
    class AddAction
    {
        [CommandLine, Route]
        public void Add(params FileInfo[] files) =>
            Console.WriteLine($"Adding files: {String.Join(", ", (object[]) files)}");
    }
}