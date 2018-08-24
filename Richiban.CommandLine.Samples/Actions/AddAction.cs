using System;
using System.IO;

namespace Richiban.CommandLine.Samples.Actions
{
    class AddAction
    {
        [CommandLine, Route]
        public void Add(params FileInfo[] files) =>
            Console.WriteLine($"Adding files: {String.Join(", ", (object[]) files)}");
    }
}