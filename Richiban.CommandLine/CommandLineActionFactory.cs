using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class CommandLineActionFactory
    {
        private readonly AssemblyModel _assemblyModel;

        public CommandLineActionFactory(AssemblyModel model)
        {
            _assemblyModel = model;
        }

        public IReadOnlyCollection<CommandLineAction> Create(CommandLineArgumentList commandLineArgs) =>
            GetMethodMappings(commandLineArgs).Select(x => x.CreateInstance()).ToList();

        private IReadOnlyCollection<MethodMapping> GetMethodMappings(CommandLineArgumentList args)
        {
            var matches = _assemblyModel
                .Select(t => MapType(args, t))
                .Choose()
                .ToList();

            return matches;
        }

        private Option<MethodMapping> MapType(
            CommandLineArgumentList args,
            MethodModel typeModel)
        {
            return MapProperties(args, typeModel)
                .IfSome(pairings => new MethodMapping(typeModel, (pairings)));
        }

        public Option<PropertyMappingList> MapProperties(
            CommandLineArgumentList args,
            MethodModel methodModel)
        {
            var parameterMappings = new List<PropertyMapping>();

            {
                if (methodModel.Verbs.Matches(args, out var argumentsMatched))
                {
                    args = args.Without(argumentsMatched);
                }
                else
                {
                    return default;
                }
            }

            args = methodModel.Parameters.ExpandShortForms(args);

            foreach (var prop in methodModel.Parameters)
            {
                var x = prop.Matches(args, out var argumentsMatched);

                x.IfSome(s => 
                {
                    parameterMappings.Add(s);
                    args = args.Without(argumentsMatched);
                });

                if(x.HasValue == false)
                {
                    return default;
                }
            }

            if (args.Any())
                return default;

            return new PropertyMappingList(parameterMappings);
        }
    }
}
