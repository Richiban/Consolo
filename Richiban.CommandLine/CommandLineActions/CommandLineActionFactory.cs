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

        public IReadOnlyCollection<CommandLineAction> Create(CommandLineArgumentList commandLineArgs) =>
            GetMethodMappings(commandLineArgs)
            .Select(mapping => new CommandLineAction(mapping, _objectFactory))
            .ToList();

        private IReadOnlyCollection<MethodMapping> GetMethodMappings(CommandLineArgumentList args)
        {
            return _assemblyModel
                .Select(t => GetMethodMapping(args, t))
                .Choose()
                .ToList();
        }

        public Option<MethodMapping> GetMethodMapping(
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

            return new MethodMapping(methodModel, parameterMappings);
        }
    }
}
