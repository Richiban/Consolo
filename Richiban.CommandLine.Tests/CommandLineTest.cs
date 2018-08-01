using System.Collections.Generic;
using System.Reflection;

namespace Richiban.CommandLine.Tests
{
    abstract class CommandLineTest
    {
        protected dynamic RunTest(params string[] args)
        {
            var config = CommandLineConfiguration.GetDefault();
            config.AssembliesToScan = new List<Assembly> { GetType().Assembly };
            return CommandLine.Execute(config, args);
        }
    }
}
