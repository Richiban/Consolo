using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Utils;

namespace Richiban.Cmdr;

internal class MethodModelBuilder
{
    private readonly CmdrAttributeDefinition _cmdrAttributeDefinition;

    public MethodModelBuilder(CmdrAttributeDefinition cmdrAttributeDefinition)
    {
        _cmdrAttributeDefinition = cmdrAttributeDefinition;
    }

    public IEnumerable<Result<MethodModelFailure, MethodModel>> BuildFrom(
        IEnumerable<IMethodSymbol?> qualifyingMethods) =>
        qualifyingMethods.SelectNotNull(TryMapMethod);

    private Result<MethodModelFailure, MethodModel> TryMapMethod(
        IMethodSymbol? methodSymbol)
    {
        if (methodSymbol is null)
        {
            return new MethodModelFailure("Method not found", location: null);
        }

        if (!methodSymbol.IsStatic)
        {
            return new MethodModelFailure(
                $"Method {methodSymbol} must be static in order to use the {_cmdrAttributeDefinition.ShortName} attribute.",
                methodSymbol.Locations.FirstOrDefault());
        }

        var parameters = methodSymbol.Parameters.Select(GetArgumentModel)
            .ToImmutableArray();

        var fullyQualifiedName = methodSymbol.ContainingType.GetFullyQualifiedName();

        var commandPath = GetCommandPath(methodSymbol);
        var description = GetDescription(methodSymbol);
        var parentNames = commandPath.Truncate(count: -1).ToList();

        var providedName = commandPath.LastOrDefault();

        return new MethodModel(
            methodSymbol.Name,
            providedName.Name,
            parentNames,
            fullyQualifiedName,
            parameters,
            description);
    }

    private string? GetDescription(IMethodSymbol methodSymbol)
    {
        if (GetRelevantAttribute(methodSymbol) is { } attr)
        {
            if (GetAttributePropertySet(attr) is { } propValue)
            {
                return propValue;
            }
        }

        return null;
    }

    private string? GetDescription(IParameterSymbol parameterSymbol)
    {
        if (GetRelevantAttribute(parameterSymbol) is { } attr)
        {
            if (GetAttributePropertySet(attr) is { } propValue)
            {
                return propValue;
            }
        }
        
        return null;
    }

    private ImmutableList<CommandPathItem> GetCommandPath(ISymbol symbol)
    {
        var path = ImmutableList.CreateBuilder<CommandPathItem>();

        while (symbol != null)
        {
            if (GetRelevantAttribute(symbol) is { } attr)
            {
                var pathItemName = 
                    GetConstructorArgument(attr) is { } arg
                        ? arg
                        : symbol.Name;

                var description = GetAttributePropertySet(attr);

                path.Add(new (pathItemName, description));
            }

            symbol = symbol.ContainingType;
        }

        path.Reverse();

        return path.ToImmutable();
    }

    private static string? GetConstructorArgument(AttributeData attributeData)
    {
        if (attributeData.ConstructorArguments.Length == 0)
        {
            return null;
        }

        return attributeData.ConstructorArguments.First() switch
        {
            { Kind: TypedConstantKind.Primitive } arg => (string?)arg.Value,
            _ => null
        };
    }

    private static string? GetAttributePropertySet(AttributeData attributeData)
    {
        if (attributeData.NamedArguments.Length == 0)
        {
            return null;
        }

        return attributeData.NamedArguments.First() switch
        {
            ("Description", { Kind: TypedConstantKind.Primitive } arg) => (string?)arg.Value,
            _ => null
        };
    }

    private AttributeData? GetRelevantAttribute(ISymbol current)
    {
        return current.GetAttributes()
            .SingleOrDefault(a => _cmdrAttributeDefinition.Matches(a.AttributeClass));
    }

    private ArgumentModel GetArgumentModel(IParameterSymbol parameterSymbol)
    {
        var name = parameterSymbol.Name;
        
        if (GetRelevantAttribute(parameterSymbol) is { } attr && GetConstructorArgument(attr) is { } arg)
        {
            name = arg;
        }

        var type = parameterSymbol.Type.GetFullyQualifiedName();
        var isFlag = type == "System.Boolean";
        var description = GetDescription(parameterSymbol);
        var isRequired = !parameterSymbol.HasExplicitDefaultValue;
        var defaultValue =
            parameterSymbol.HasExplicitDefaultValue
            ? SourceValueUtils.SourceValue(parameterSymbol.ExplicitDefaultValue)
            : null;

        return new ArgumentModel(
            name,
            type,
            isFlag,
            IsRequired: isRequired,
            DefaultValue: defaultValue,
            Description: description);
    }
}
