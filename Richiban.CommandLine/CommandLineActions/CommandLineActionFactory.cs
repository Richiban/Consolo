using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class CommandLineActionFactory
    {
        private readonly AssemblyModel _assemblyModel;
        private readonly IObjectFactory _objectFactory;

        public CommandLineActionFactory(AssemblyModel model, IObjectFactory objectFactory)
        {
            _assemblyModel = model;
            _objectFactory = objectFactory;
        }

        public IReadOnlyCollection<CommandLineAction> Create(CommandLineArgumentList commandLineArgs)
        {
            var (explicitMatches, implicitMatches) =
                GetMethodMappings(commandLineArgs)
                .Partition(mapping => mapping.MatchDisambiguation == MatchDisambiguation.ExplicitMatch);

            var mappings =
                explicitMatches.Any()
                ? explicitMatches
                : implicitMatches;

            return mappings
                .Select(mapping => new CommandLineAction(mapping, _objectFactory))
                .ToList();
        }

        private IReadOnlyCollection<MethodMapping> GetMethodMappings(CommandLineArgumentList args) =>
            _assemblyModel
                .Select(model => model.GetMethodMapping(args))
                .Choose()
                .ToList();
    }
}
