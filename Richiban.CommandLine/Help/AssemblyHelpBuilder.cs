using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Richiban.CommandLine
{
    class AssemblyHelpBuilder
    {
        private readonly MethodHelpBuilder _methodHelpBuilder;

        public AssemblyHelpBuilder(MethodHelpBuilder methodHelpBuilder)
        {
            _methodHelpBuilder = methodHelpBuilder;
        }

        public string GenerateHelp(IEnumerable<MethodModel> model, CommandLineArgumentList commandLineArgs)
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
                    .AllByMax(m => m.GetPartialMatchAccuracy(commandLineArgs));
            }

            return GenerateHelp(modelsForHelp);
        }

        public string GenerateHelp(IEnumerable<CommandLineAction> actions)
        {
            return GenerateHelp(actions.Select(a => a.Model));
        }

        private string GenerateHelp(IEnumerable<MethodModel> models)
        {
            var sb = new StringBuilder();

            sb.Append(
                string.Join($"{Environment.NewLine}{Environment.NewLine}",
                models.Select(t => _methodHelpBuilder.BuildFor(t))));

            return sb.ToString();
        }
    }
}
