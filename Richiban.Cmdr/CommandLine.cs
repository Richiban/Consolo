using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Richiban.Cmdr
{
    /// <summary>
    ///     The entrypoints for Richiban.Cmdr
    /// </summary>
    public static class CommandLine
    {
        private static Action<string> _traceOutput = s => Debug.WriteLine(s);

        /// <summary>
        ///     The default entrypoint for Richiban.Cmdr
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>The object returned by the target method (or null if the method was void)</returns>
        public static object Execute(params string[] args) =>
            Execute(CommandLineConfiguration.GetDefault(), args);

        /// <summary>
        ///     The entrypoint for Richiban.Cmdr that supports custom configuration
        /// </summary>
        /// <param name="config">The CommandLine configuration</param>
        /// <param name="args">The command line arguments</param>
        /// <returns>The object returned by the target method (or null if the method was void)</returns>
        public static object Execute(
            CommandLineConfiguration config,
            params string[] args)
        {
            // Defensively copy the configuration into local variables
            var helpOutput = config.HelpOutput;
            _traceOutput = config.TraceOutput;
            var assembliesToScan = config.AssembliesToScan.ToList();
            var typeConverters = new TypeConverterCollection(config.TypeConverters);
            var objectFactory = config.ObjectFactory;

            var commandLineArgs = CommandLineArgumentList.Parse(args);

            if (commandLineArgs.TraceToStandardOutput)
            {
                _traceOutput = helpOutput;
            }

            var model = AssemblyModel.Scan(assembliesToScan);

            var modelText = JsonSerializer.Serialize(model);

            var path =
                $@"C:\Source\Richiban\CommandLine\Richiban.Cmdr.Samples\{DateTime.Now:yyyy-MM-dd_hh-mm-ss}.json";

            File.WriteAllText(path, modelText);

            var resolvedCommandLineActions = ResolveActions(
                objectFactory,
                typeConverters,
                commandLineArgs,
                model);

            if (IsReadyForExecution(commandLineArgs, resolvedCommandLineActions))
            {
                var commandLineAction = resolvedCommandLineActions.Single();

                try
                {
                    return commandLineAction.Invoke();
                }
                catch (TypeConversionException e)
                {
                    helpOutput(e.Message);
                    _traceOutput(e.ToString());

                    return null;
                }
            }

            var helpBuilder =
                new HelpBuilder(XmlCommentsRepository.LoadFor(assembliesToScan));

            helpOutput(
                helpBuilder.GenerateHelp(
                    commandLineArgs,
                    model,
                    resolvedCommandLineActions));

            return null;
        }

        private static IReadOnlyCollection<CommandLineAction> ResolveActions(
            Func<Type, object> objectFactory,
            TypeConverterCollection typeConverters,
            CommandLineArgumentList commandLineArgs,
            AssemblyModel model)
        {
            var methodMapper = new MethodMapper(new ParameterMapper());

            return new CommandLineActionFactory(
                model,
                objectFactory,
                typeConverters,
                methodMapper).Resolve(commandLineArgs);
        }

        private static bool IsReadyForExecution(
            CommandLineArgumentList commandLineArgs,
            IReadOnlyCollection<CommandLineAction> resolvedCommandLineActions) =>
            !(commandLineArgs.IsCallForHelp || resolvedCommandLineActions.Count != 1);

        internal static void Trace(object message, int indentationLevel = 0)
        {
            var indentation = string.Concat(
                Enumerable.Repeat(element: 0, indentationLevel * 4).Select(_ => " "));

            _traceOutput($"{indentation}{message}");
        }
    }
}