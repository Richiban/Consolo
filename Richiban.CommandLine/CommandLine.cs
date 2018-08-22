using NullGuard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Richiban.CommandLine
{
    /// <summary>
    /// The entrypoints for Richiban.CommandLine
    /// </summary>
    public static class CommandLine
    {
        internal static CommandLineConfiguration CurrentConfiguration { get; private set; }

        /// <summary>
        /// The default entrypoint for Richiban.CommandLine
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>The object returned by the target method (or null if the method was void)</returns>
        [return: AllowNull]
        public static object Execute(params string[] args) => 
            Execute(CommandLineConfiguration.GetDefault(), args);

        /// <summary>
        /// The entrypoint for Richiban.CommandLine that supports custom configuration
        /// </summary>
        /// <param name="config">The CommandLine configuration</param>
        /// <param name="args">The command line arguments</param>
        /// <returns>The object returned by the target method (or null if the method was void)</returns>
        [return:AllowNull]
        [TracerAttributes.TraceOn]
        public static object Execute(CommandLineConfiguration config, params string[] args)
        {
            CurrentConfiguration = config;

            var commandLineArgs = CommandLineArgumentList.Parse(args);

            if (commandLineArgs.TraceToStandardOutput)
            {
                config.TraceOutput = config.HelpOutput;
            }

            var model = AssemblyModel.Scan(config.AssembliesToScan);
            var resolvedCommandLineActions = ResolveActions(config, commandLineArgs, model);

            if (IsReadyForExecution(commandLineArgs, resolvedCommandLineActions))
            {
                var commandLineAction = resolvedCommandLineActions.Single();

                try
                {
                    return commandLineAction.Invoke();
                }
                catch (TypeConversionException e)
                {
                    config.HelpOutput(e.Message);
                    config.TraceOutput(e.ToString());

                    return null;
                }
            }
            else
            {
                var helpBuilder = new HelpBuilder(XmlCommentsRepository.LoadFor(config.AssembliesToScan));

                config.HelpOutput(
                    helpBuilder.GenerateHelp(commandLineArgs, model, resolvedCommandLineActions));

                return null;
            }
        }

        private static IReadOnlyCollection<CommandLineAction> ResolveActions(CommandLineConfiguration config, CommandLineArgumentList commandLineArgs, AssemblyModel model)
        {
            var typeConverterCollection = new TypeConverterCollection(config.TypeConverters);
            var methodMapper = new MethodMapper(new ParameterMapper());

            var resolvedCommandLineActions =
                new CommandLineActionFactory(
                    model, config.ObjectFactory, typeConverterCollection, methodMapper)
                .Resolve(commandLineArgs);
            return resolvedCommandLineActions;
        }

        private static bool IsReadyForExecution(
            CommandLineArgumentList commandLineArgs,
            IReadOnlyCollection<CommandLineAction> resolvedCommandLineActions)
        {
            return !(commandLineArgs.IsCallForHelp || resolvedCommandLineActions.Count != 1);
        }

        internal static void Trace(object message, int indentationLevel = 0)
        {
            var indentation = String.Concat(Enumerable.Repeat(0, indentationLevel).Select(_ => "\t"));
            var fullMessage = $"{indentation}{message}";

            var output = CurrentConfiguration?.TraceOutput ?? (s => Debug.WriteLine(s));

            output(fullMessage);
        }
    }
}
