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
            var scanner = new Scanner(Assembly.GetEntryAssembly());

            var model = scanner.BuildModel();
            
            new CommandLineActionFactory(model)
                .Create(CommandLineArgumentList.Parse(args))
                .Match(
                    None: () => Console.WriteLine(GenerateHelp(model)),
                    Some: commandLineAction => commandLineAction.Execute());
        }

        private static string GenerateHelp(IEnumerable<TypeModel> implementingTypes) => 
            String.Join(Environment.NewLine, implementingTypes.Select(t => t.Help));
    }
}
