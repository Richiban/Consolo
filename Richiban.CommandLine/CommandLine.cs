using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Richiban.CommandLine
{
    public static class CommandLine
    {
        public static void Execute(params string[] args)
        {
            Execute(args, Console.WriteLine);
        }

        public static void Execute(string[] args, Action<string> helpOutput)
        {
            var model = AssemblyModel.Scan(Assembly.GetEntryAssembly());
            var commandLineArgs = CommandLineArgumentList.Parse(args);

            var commandLineActions = new CommandLineActionFactory(model)
                .Create(commandLineArgs);

            if (commandLineArgs.IsCallForHelp)
            {
                if(commandLineArgs.Count == 0)
                {
                    helpOutput(GenerateHelp(model));
                }

                foreach (var help in commandLineActions.Select(x => x.Help))
                {
                    helpOutput(help);
                }

                return;
            }

            switch(commandLineActions.Count)
            {
                case 0:
                    throw new Exception("Could not match the given arguments to a command");
                case 1:
                    commandLineActions.Single().Execute();
                    break;
                default:
                    throw new Exception("The given arguments are ambigous");
            }
        }

        private static string GenerateHelp(IEnumerable<MethodModel> implementingTypes) => 
            String.Join($"{Environment.NewLine}\t", implementingTypes.Select(t => t.Help));
    }
}
