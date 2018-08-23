using System;
using System.IO;

namespace Richiban.CommandLine.Samples.Actions
{
    class AddAction
    {
        [CommandLine, Route]
        public void Add(params FileInfo[] files) =>
            $"Adding files: {String.Join(", ", (object[]) files)}".Dump();
    }
}