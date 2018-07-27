using System.IO;

namespace Richiban.CommandLine.Samples.Actions
{
    class AddAction
    {
        [CommandLine, Verb]
        public void Add(FileInfo file)
        {
            $"Adding file {file}".Dump();
        }
    }
}
