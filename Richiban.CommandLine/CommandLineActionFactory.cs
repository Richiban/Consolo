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

        public Option<CommandLineAction> Create(CommandLineArgumentList commandLineArgs) =>
            GetTypeMapping(commandLineArgs).IfSome(x => x.CreateInstance());

        private Option<MethodMapping> GetTypeMapping(CommandLineArgumentList args)
        {
            var matchingTypes = _assemblyModel
                .Select(t => MapType(args, t))
                .Choose()
                .ToList();

            switch (matchingTypes.Count)
            {
                case 0:
                    return default;
                case 1:
                    return matchingTypes.Single();
                case var n:
                    var bestMatch =
                        matchingTypes.GroupBy(m => m.PropertyMappings.Count)
                        .OrderByDescending(m => m.Key)
                        .First()
                        .ToList();

                    if (bestMatch.Count() > 1)
                    {
                        return default;
                    }
                    else
                    {
                        return bestMatch.Single();
                    }
            }
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
