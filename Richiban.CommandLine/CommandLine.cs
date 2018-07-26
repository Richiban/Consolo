using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Richiban.CommandLine
{
    public class CommandLine
    {
        private readonly IObjectFactory _objectFactory;
        private readonly Action<string> _helpOutput;

        public CommandLine(IObjectFactory objectFactory, Action<string> helpOutput)
        {
            _objectFactory = objectFactory;
            _helpOutput = helpOutput;
        }

        public static void Execute(params string[] args)
        {
            Execute(args, Console.WriteLine);
        }

        public static void Execute(string[] args, Action<string> helpOutput)
        {
            var commandLine = new CommandLine(new SystemActivatorObjectFactory(), helpOutput);

            commandLine.Execute2(args);
        }

        public void Execute2(string[] args)
        {
            var model = AssemblyModel.Scan(Assembly.GetEntryAssembly());
            var commandLineArgs = CommandLineArgumentList.Parse(args);

            var commandLineActions = new CommandLineActionFactory(model, _objectFactory)
                .Create(commandLineArgs);

            if (commandLineArgs.IsCallForHelp)
            {
                if(commandLineArgs.Count == 0)
                {
                    _helpOutput(GenerateHelp(model));
                    return;
                }

                foreach (var help in commandLineActions.Select(x => x.Help))
                {
                    _helpOutput(help);
                }

                return;
            }

            switch(commandLineActions.Count)
            {
                case 0:
                    _helpOutput(GenerateHelp(model));
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
