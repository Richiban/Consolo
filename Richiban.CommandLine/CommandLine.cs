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

            try
            {
                var commandLineAction = new CommandLineActionFactory(model)
                    .Create(CommandLineArgumentCollection.Parse(args));

                commandLineAction.Execute();
            }
            catch (Exception)
            {
                Console.WriteLine(GenerateHelp(model));
            }
        }

        private static string GenerateHelp(IEnumerable<TypeModel> implementingTypes) => 
            String.Join(Environment.NewLine, implementingTypes.Select(t => t.Help));
    }
}
