using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Richiban.Cmdr.Models;

namespace Richiban.Cmdr
{
    internal class MethodModelBuilder
    {
        private static readonly SymbolDisplayFormat SymbolDisplayFormat =
            new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle
                .NameAndContainingTypesAndNamespaces);

        private readonly CmdrAttributeDefinition _cmdrAttributeDefinition;

        public MethodModelBuilder(CmdrAttributeDefinition cmdrAttributeDefinition)
        {
            _cmdrAttributeDefinition = cmdrAttributeDefinition;
        }

        public IEnumerable<Result<MethodModelFailure, MethodModel>> BuildFrom(
            IEnumerable<IMethodSymbol?> qualifyingMethods) =>
            qualifyingMethods.Choose(TryMapMethod);

        private Result<MethodModelFailure, MethodModel> TryMapMethod(
            IMethodSymbol? methodSymbol)
        {
            if (methodSymbol is null)
            {
                return new MethodModelFailure($"Method not found", location: null);
            }

            if (!methodSymbol.IsStatic)
            {
                return new MethodModelFailure(
                    $"Method {methodSymbol} must be static in order to use the {_cmdrAttributeDefinition.ShortName} attribute.",
                    methodSymbol.Locations.FirstOrDefault());
            }

            var parameters = methodSymbol.Parameters.Select(GetArgumentModel)
                .ToImmutableArray();

            var fullyQualifiedName =
                GetFullyQualifiedTypeName(methodSymbol.ContainingType);

            var commandPath = GetCommandPath(methodSymbol);

            var parentNames = commandPath.Truncate(-1).ToList();

            var providedName = commandPath.LastOrDefault();

            return new MethodModel(
                methodSymbol.Name,
                providedName,
                parentNames,
                fullyQualifiedName,
                parameters);
        }

        private ImmutableList<string> GetCommandPath(ISymbol symbol)
        {
            var path = ImmutableList.CreateBuilder<string>();

            while (symbol != null)
            {
                if (GetRelevantAttribute(symbol) is { } attr)
                {
                    if (GetConstructorArgument(attr) is { } arg)
                    {
                        path.Add(arg);
                    }
                    else
                    {
                        path.Add(symbol.Name);
                    }
                }

                symbol = symbol.ContainingType;
            }

            path.Reverse();

            return path.ToImmutable();
        }

        private static string? GetConstructorArgument(AttributeData attributeData)
        {
            if (attributeData.ConstructorArguments.Length == 0)
                return null;

            return attributeData.ConstructorArguments.First() switch
            {
                { Kind: TypedConstantKind.Primitive } arg => (string?) arg.Value,
                _ => null
            };
        }

        private AttributeData? GetRelevantAttribute(ISymbol current)
        {
            return current.GetAttributes()
                .SingleOrDefault(
                    a => a.AttributeClass?.ToString() == _cmdrAttributeDefinition.FullyQualifiedName);
        }

        private static string GetFullyQualifiedTypeName(ITypeSymbol containingType) =>
            containingType.ToDisplayString(SymbolDisplayFormat);

        private static ArgumentModel GetArgumentModel(IParameterSymbol parameterSymbol)
        {
            var name = parameterSymbol.Name;
            var type = GetFullyQualifiedTypeName(parameterSymbol.Type);
            var isFlag = type == "System.Boolean";

            return new ArgumentModel(name, type, isFlag);
        }
    }
}