using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class CommandLineActionFactory
    {
        private readonly AssemblyModel _assemblyModel;
        private readonly Func<Type, object> _objectFactory;

        public CommandLineActionFactory(AssemblyModel model, Func<Type, object> objectFactory)
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

            if (bestGroup == null)
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

        private static CommandLineAction CreateAction(
            MethodMapping methodMapping,
            Func<Type, object> objectFactory) =>
            new CommandLineAction(() =>
            {
                var instance = CreateInstanceOfDeclaringType(methodMapping.MethodModel, objectFactory);

                var methodArguments = methodMapping
                    .Select(paramMapping => 
                        TypeConverter.ConvertValue(paramMapping.ConvertToType, paramMapping.SuppliedValue))
                    .ToArray();

                return methodMapping.MethodModel.InvokeFunc(
                    instance,
                    methodArguments);
            },
            methodMapping.MethodModel.Help);

        private static object CreateInstanceOfDeclaringType(
            MethodModel methodModel, Func<Type, object> objectFactory)
        {
            if (methodModel.IsStatic)
                return null;

            return objectFactory(methodModel.DeclaringType);
        }
    }
}
