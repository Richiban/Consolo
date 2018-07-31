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
            var model = AssemblyModel.Scan(config.AssembliesToScan);
            var commandLineArgs = CommandLineArgumentList.Parse(args);
            var typeConverterCollection = new TypeConverterCollection(config.TypeConverters);

            var commandLineActions = 
                new CommandLineActionFactory(model, config.ObjectFactory, typeConverterCollection)
                .Create(commandLineArgs);

            if (commandLineArgs.IsCallForHelp)
            {
                config.HelpOutput(GenerateHelp(model, commandLineArgs));

                return null;
            }

            if (commandLineActions.Count == 0)
            {
                config.HelpOutput("Could not match the given arguments to a command");

                config.HelpOutput(GenerateHelp(model, commandLineArgs));

                return null;
            }

            if (commandLineActions.Count > 1)
            {
                config.HelpOutput("The given arguments are ambiguous between the following:");

                config.HelpOutput(GenerateHelp(commandLineActions));

                return null;
            }

            return commandLineActions.Single().Invoke();
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
    }
}
