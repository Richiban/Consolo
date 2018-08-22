using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class CommandLineActionFactory
    {
        private readonly AssemblyModel _assemblyModel;
        private readonly Func<Type, object> _objectFactory;
        private readonly TypeConverterCollection _typeConverterCollection;
        private readonly MethodMapper _methodMapper;

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

        [TracerAttributes.TraceOn]
        public IReadOnlyCollection<CommandLineAction> Resolve(CommandLineArgumentList commandLineArgs) =>
            GetBestMatches(commandLineArgs)
                .Select(mapping => CreateAction(mapping, _objectFactory))
                .ToList();

        [TracerAttributes.TraceOn]
        private IReadOnlyCollection<MethodMapping> GetBestMatches(CommandLineArgumentList commandLineArgs)
        {
            var matchGroups =
                GetMethodMappings(commandLineArgs)
                .ToLookup(mapping => mapping.MatchDisambiguation)
                .OrderByDescending(group => group.Key);

            return
                matchGroups
                .FirstOrDefault(group => group.Any())
                ?.ToList()
                ?? new List<MethodMapping>();
        }

        [TracerAttributes.TraceOn]
        private IReadOnlyCollection<MethodMapping> GetMethodMappings(CommandLineArgumentList args) =>
            _assemblyModel
                .Select(model => _methodMapper.GetMethodMapping(model, args))
                .Choose()
                .ToList();

        private CommandLineAction CreateAction(
            MethodMapping methodMapping,
            Func<Type, object> objectFactory) =>
            new CommandLineAction(() =>
                {
                    var instance = CreateInstanceOfDeclaringType(methodMapping.MethodModel, objectFactory);

                    var methodArguments = methodMapping
                        .Select(paramMapping =>
                            _typeConverterCollection.ConvertValue(
                                paramMapping.ConvertToType,
                                paramMapping.SuppliedValues))
                        .ToArray();

                    return methodMapping.MethodModel.InvokeFunc(
                        instance,
                        methodArguments);
                },
                methodMapping.MethodModel);

        private static object CreateInstanceOfDeclaringType(
            MethodModel methodModel, Func<Type, object> objectFactory)
        {
            if (methodModel.IsStatic)
                return null;

            return objectFactory(methodModel.DeclaringType);
        }
    }
}
