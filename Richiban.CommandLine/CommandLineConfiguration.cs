using System;
using System.Reflection;

namespace Richiban.CommandLine
{
    public class CommandLineConfiguration
    {
        public Action<string> HelpOutput { get; set; }
        public IObjectFactory ObjectFactory { get; set; }
        public Assembly AssemblyToScan { get; set; }

        public static CommandLineConfiguration Default { get; } =
            new CommandLineConfiguration
            {
                ObjectFactory = new SystemActivatorObjectFactory(),
                HelpOutput = Console.WriteLine,
                AssemblyToScan = Assembly.GetEntryAssembly()
            };
    }
}
