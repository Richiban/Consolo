using NullGuard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Richiban.CommandLine
{
    public class CommandLine
    {
        public static void Execute(params string[] args) => 
            Execute(CommandLineConfiguration.GetDefault(), args);

        [return:AllowNull]
        public static object Execute(CommandLineConfiguration config, params string[] args)
        {
            var model = AssemblyModel.Scan(config.AssemblyToScan);
            var commandLineArgs = CommandLineArgumentList.Parse(args);

            var commandLineActions = new CommandLineActionFactory(model, config.ObjectFactory)
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
