using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.Cmdr
{
    internal class CommandLineActionFactory
    {
        private readonly AssemblyModel _assemblyModel;
        private readonly MethodMapper _methodMapper;
        private readonly Func<Type, object> _objectFactory;
        private readonly TypeConverterCollection _typeConverterCollection;

        public CommandLineActionFactory(
            AssemblyModel model,
            Func<Type, object> objectFactory,
            TypeConverterCollection typeConverterCollection,
            MethodMapper methodMapper)
        {
            _assemblyModel = model;
            _objectFactory = objectFactory;
            _typeConverterCollection = typeConverterCollection;
            _methodMapper = methodMapper;
        }

        public IReadOnlyCollection<CommandLineAction> Resolve(
            CommandLineArgumentList commandLineArgs) =>
            GetBestMatches(commandLineArgs)
                .Select(mapping => CreateAction(mapping, _objectFactory))
                .ToList();

        private IReadOnlyCollection<MethodMapping> GetBestMatches(
            CommandLineArgumentList commandLineArgs)
        {
            var matchGroups = GetMethodMappings(commandLineArgs)
                .ToLookup(mapping => mapping.MatchPriority)
                .OrderByDescending(group => group.Key);

            return matchGroups.FirstOrDefault(group => group.Any())?.ToList() ??
                   new List<MethodMapping>();
        }

        private IReadOnlyCollection<MethodMapping>
            GetMethodMappings(CommandLineArgumentList args) =>
            _assemblyModel.Select(model => _methodMapper.GetMethodMapping(model, args))
                .Choose()
                .ToList();

        private CommandLineAction CreateAction(
            MethodMapping methodMapping,
            Func<Type, object> objectFactory) =>
            new(
                () =>
                {
                    var instance = CreateInstanceOfDeclaringType(
                        methodMapping.MethodModel,
                        objectFactory);

                    var methodArguments = methodMapping.Select(
                            paramMapping => _typeConverterCollection.ConvertValue(
                                /*TODO paramMapping.ConvertToType*/ typeof(string),
                                paramMapping.SuppliedValues))
                        .ToArray();

                    return methodMapping.MethodModel.InvokeFunc(
                        instance,
                        methodArguments);
                },
                methodMapping.MethodModel);

        private static object CreateInstanceOfDeclaringType(
            MethodModel methodModel,
            Func<Type, object> objectFactory)
        {
            if (methodModel.IsStatic)
            {
                return null;
            }

            return objectFactory(typeof(object) /*TODO methodModel.DeclaringType*/);
        }
    }
}