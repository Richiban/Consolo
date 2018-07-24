using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    public static class CommandLine
    {
        public static void Execute(params string[] args)
        {
            var scanner = new Scanner();

            try
            {
                scanner.Scan(CommandLineArgumentCollection.Parse(args));
            }
            catch (Exception)
            {
                Console.WriteLine(GenerateHelp(scanner.ImplementingTypes));
            }
        }

        private static string GenerateHelp(IEnumerable<TypeModel> implementingTypes) => 
            String.Join(Environment.NewLine, implementingTypes.Select(t => t.Help));
    }
}
