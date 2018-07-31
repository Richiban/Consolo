using System;
using System.Reflection;

namespace Richiban.CommandLine
{
    public class CommandLineConfiguration
    {
        /// <summary>
        /// Provide an action for the output from generating help. Defaults to Console.WriteLine.
        /// </summary>
        public Action<string> HelpOutput { get; set; }

        /// <summary>
        /// The component responsible for instantiating the classes that contain CommandLine methods.
        /// 
        /// Defaults to an implementation using System.Activator, replace with the resolve method of your
        /// favourite DI container to enable dependency injection.
        /// </summary>
        public Func<Type, object> ObjectFactory { get; set; }

        /// <summary>
        /// The assembly to scan for CommandLine methods.
        /// 
        /// Defaults to Assembly.GetEntryAssembly().
        /// </summary>
        public Assembly AssemblyToScan { get; set; }

        public static CommandLineConfiguration GetDefault() =>
            new CommandLineConfiguration
            {
                ObjectFactory = new SystemActivatorObjectFactory(),
                HelpOutput = Console.WriteLine,
                AssemblyToScan = Assembly.GetEntryAssembly()
            };
    }
}
