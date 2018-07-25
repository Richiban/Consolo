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
            var model = AssemblyModel.Scan(Assembly.GetEntryAssembly());
            var commandLineArgs = CommandLineArgumentList.Parse(args);
            
            new CommandLineActionFactory(model)
                .Create(commandLineArgs)
                .Match(
                    None: () => Console.WriteLine(GenerateHelp(model)),
                    Some: commandLineAction => commandLineAction.Execute());
        }

        private static string GenerateHelp(IEnumerable<MethodModel> implementingTypes) => 
            String.Join(Environment.NewLine, implementingTypes.Select(t => t.Help));
    }
}
