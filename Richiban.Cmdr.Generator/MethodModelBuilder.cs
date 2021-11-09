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
        private static readonly SymbolDisplayFormat SymbolDisplayFormat = new(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle
                .NameAndContainingTypesAndNamespaces);

        private readonly CmdrAttribute _cmdrAttribute;

        public MethodModelBuilder(CmdrAttribute cmdrAttribute)
        {
            _cmdrAttribute = cmdrAttribute;
        }

        public IEnumerable<Result<MethodModelFailure, MethodModel>> BuildFrom(
            IEnumerable<IMethodSymbol?> qualifyingMethods) =>
            qualifyingMethods.Choose(TryMapMethod);

        private Result<MethodModelFailure, MethodModel> TryMapMethod(IMethodSymbol? methodSymbol)
        {
            if (methodSymbol is null)
            {
                return new MethodModelFailure($"Method not found", location: null);
            }

            if (!methodSymbol.IsStatic)
            {
                return new MethodModelFailure(
                    $"Method {methodSymbol} must be static in order to use the {_cmdrAttribute.AttributeName} attribute.",
                    methodSymbol.Locations.FirstOrDefault());
            }

            var parameters = methodSymbol.Parameters.Select(GetArgumentModel)
                .ToImmutableArray();

            var parentNames = GetAttributeArguments(methodSymbol);

            var fullyQualifiedName =
                GetFullyQualifiedTypeName(methodSymbol.ContainingType);

            var aliases = GetAliases(methodSymbol);

            return new MethodModel(
                methodSymbol.Name,
                aliases,
                parentNames,
                fullyQualifiedName,
                parameters);
        }

        private string[] GetAliases(IMethodSymbol methodSymbol) =>
            GetRelevantAttribute(methodSymbol) switch
            {
                null => Array.Empty<string>(),
                var attr => GetConstructorArguments(attr)
            };

        private ImmutableArray<string> GetAttributeArguments(IMethodSymbol methodSymbol)
        {
            return GetRelevantAttributesAllTheWayUp(methodSymbol)
                .SelectMany(GetConstructorArguments)
                .ToImmutableArray();
        }

        private static string[] GetConstructorArguments(AttributeData attributeData)
        {
            if (attributeData.ConstructorArguments.Length == 0)
                return Array.Empty<string>();

            return attributeData.ConstructorArguments.First() switch
            {
                { Kind: TypedConstantKind.Primitive, Value: var val } => new[]
                    {
                        (string?)val
                    }.Choose(x => x)
                    .ToArray(),
                { Kind: TypedConstantKind.Array, Values: var vals } => vals
                    .Choose(x => (string?)x.Value)
                    .ToArray(),
                _ => Array.Empty<string>()
            };
        }

        private Stack<AttributeData> GetRelevantAttributesAllTheWayUp(
            IMethodSymbol methodSymbol)
        {
            var attributes = new Stack<AttributeData>();

            var current = (ISymbol)methodSymbol;

            while (current != null)
            {
                if (GetRelevantAttribute(current) is { } attr)
                {
                    attributes.Push(attr);
                }

                current = current.ContainingType;
            }

            return attributes;
        }

        private AttributeData? GetRelevantAttribute(ISymbol current)
        {
            return current.GetAttributes()
                .SingleOrDefault(
                    a => a.AttributeClass?.Name == _cmdrAttribute.AttributeName);
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