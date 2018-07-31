using System;
using System.Collections.Generic;
using System.Reflection;

namespace Richiban.CommandLine
{
    /// <summary>
    /// The class through which the behaviour and functionality of Richiban.CommandLine can be customised
    /// </summary>
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
        /// The assemblies to scan for CommandLine methods.
        /// 
        /// Defaults to Assembly.GetEntryAssembly(). Feel free to add more or replace entirely.
        /// </summary>
        public List<Assembly> AssembliesToScan { get; set; }

        /// <summary>
        /// For each value supplied at the command line for a given parameter, we loop through a stack
        /// of <see cref="ITypeConverter"/> instances until we find one that can convert the raw values
        /// into an object of the correct type.
        /// 
        /// You can push your own implementation of an ITypeConverter onto this stack, or replace the
        /// stack completely.
        /// </summary>
        public Stack<ITypeConverter> TypeConverters { get; set; }

        /// <summary>
        /// Create a new instance of CommandLineConfiguration with properties set to the default values
        /// </summary>
        /// <returns>A new CommandLineConfiguration object</returns>
        public static CommandLineConfiguration GetDefault() =>
            new CommandLineConfiguration
            {
                ObjectFactory = new SystemActivatorObjectFactory(),
                HelpOutput = Console.WriteLine,
                AssembliesToScan = new List<Assembly> { Assembly.GetEntryAssembly() },
                TypeConverters = GetDefaultTypeConverters()
            };

        private static Stack<ITypeConverter> GetDefaultTypeConverters()
        {
            var stack = new Stack<ITypeConverter>();

            stack.Push(new ConstructFromStringTypeConverter());
            stack.Push(new SystemConvertibleTypeConverter());
            stack.Push(new EnumTypeConverter());
            stack.Push(new StringPassthroughTypeConverter());
            stack.Push(new MissingValueTypeConverter());

            return stack;
        }
    }
}
