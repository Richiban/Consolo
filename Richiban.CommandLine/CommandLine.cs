using NullGuard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public static object Execute(CommandLineConfiguration config, params string[] args)
        {
            CurrentConfiguration = config;

            Log("Lexing commandline arguments");
            var commandLineArgs = CommandLineArgumentList.Parse(args);

            if(commandLineArgs.RedirectDiagnosticsToStandardOutput)
            {
                config.DebugOutput = config.HelpOutput;
            }

            var model = AssemblyModel.Scan(config.AssembliesToScan);

            var typeConverterCollection = new TypeConverterCollection(config.TypeConverters);
            var methodMapper = new MethodMapper(new ParameterMapper());

            Log("Creating CommandLineActions");
            var commandLineActions = 
                new CommandLineActionFactory(
                    model, config.ObjectFactory, typeConverterCollection, methodMapper)
                .Create(commandLineArgs);

            if (commandLineArgs.IsCallForHelp)
            {
                Log(new { commandLineArgs.IsCallForHelp });

                Log("Generating help");
                var help = GenerateHelp(model, commandLineArgs);
                Log(help);
                config.HelpOutput(help);

                return null;
            }

            if (commandLineActions.Count == 0)
            {
                Log(new { commandLineActionsCount = commandLineActions.Count });
                config.HelpOutput("Could not match the given arguments to a command");

                Log("Generating help");
                var help = GenerateHelp(model, commandLineArgs);
                Log(help);
                config.HelpOutput(help);

                return null;
            }

            if (commandLineActions.Count > 1)
            {
                Log(new { commandLineActionsCount = commandLineActions.Count });
                config.HelpOutput("The given arguments are ambiguous between the following:");

                Log("Generating help");
                var help = GenerateHelp(commandLineActions);
                Log(help);
                config.HelpOutput(help);

                return null;
            }

            var commandLineAction = commandLineActions.Single();
            Log("Found target method");
            Log(commandLineAction.Help);

            Log("Executing target method");
            return commandLineAction.Invoke();
        }

        private static string GenerateHelp(IEnumerable<MethodModel> model, CommandLineArgumentList commandLineArgs)
        {
            var sb = new StringBuilder();

            var modelsForHelp = model;

            if (commandLineArgs.Count == 0)
            {
                sb.AppendLine($"Usage:");
            }
            else
            {
                sb.AppendLine($"Help for {commandLineArgs}:");

                modelsForHelp = modelsForHelp
                    .MaxByAll(m => m.GetPartialMatchAccuracy(commandLineArgs));
            }

            sb.Append(
                string.Join($"{Environment.NewLine}{Environment.NewLine}",
                modelsForHelp
                .Select(t => $"\t{AppDomain.CurrentDomain.FriendlyName} {t.Help}")));

            return sb.ToString();
        }

        private static string GenerateHelp(IEnumerable<CommandLineAction> actions)
        {
            var sb = new StringBuilder();

            sb.Append(
                string.Join($"{Environment.NewLine}{Environment.NewLine}",
                actions
                .Select(act => $"\t{AppDomain.CurrentDomain.FriendlyName} {act.Help}")));

            return sb.ToString();
        }

        internal static void Log(object message, int indentationLevel = 0)
        {
            var indentation = String.Concat(Enumerable.Repeat(0, indentationLevel).Select(_ => "\t"));
            var fullMessage = $"[Debug {DateTime.Now:HH:mm:ss}] {indentation}{message}";

            CurrentConfiguration.DebugOutput(fullMessage);
        }
    }
}
