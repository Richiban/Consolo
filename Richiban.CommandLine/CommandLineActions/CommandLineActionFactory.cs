using System;
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
            var matchGroups =
                GetMethodMappings(commandLineArgs)
                .ToLookup(mapping => mapping.MatchDisambiguation)
                .OrderByDescending(group => group.Key);

            var bestGroup =
                matchGroups
                .FirstOrDefault(group => group.Any());

            if(bestGroup == null)
            {
                return new List<CommandLineAction>();
            }

            return
                bestGroup
                .Select(mapping => CreateAction(mapping, _objectFactory))
                .ToList();
        }

        private IReadOnlyCollection<MethodMapping> GetMethodMappings(CommandLineArgumentList args) =>
            _assemblyModel
                .Select(model => model.GetMethodMapping(args))
                .Choose()
                .ToList();

        private static CommandLineAction CreateAction(MethodMapping methodMapping, IObjectFactory objectFactory) => 
            new CommandLineAction(() =>
            {
                var instance = CreateInstanceOfDeclaringType(methodMapping, objectFactory);

                var methodArguments = methodMapping
                    .Select(m => TypeConverter.ConvertValue(m.ConvertToType, m.SuppliedValue))
                    .ToArray();

                return methodMapping.MethodModel.InvokeFunc(
                    instance,
                    methodArguments);
            },
            methodMapping.MethodModel.Help);

        private static object CreateInstanceOfDeclaringType(
            MethodMapping methodMapping, IObjectFactory objectFactory)
        {
            if (methodMapping.IsStatic)
                return null;

            var declaringType = methodMapping.MethodModel.DeclaringType;

            return objectFactory.CreateInstance(declaringType);
        }
    }
}
