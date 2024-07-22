using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Consolo;

public static class AttributeUsageUtils
{
    public static ConsoloAttributeUsage? GetUsage(ISymbol symbol)
    {
        var attributeData = symbol.GetAttributes()
            .FirstOrDefault(attr => ConsoloAttributeDefinition.Matches(attr.AttributeClass));

        if (attributeData is null)
        {
            return null;
        }

        var names = attributeData.ConstructorArguments
            .SelectMany(arg => arg.Values.Select(val => val.Value?.ToString())
            .Where(val => val is not null))
            .ToImmutableArray();

        var shortForm = attributeData.NamedArguments
            .FirstOrDefault(kvp => kvp.Key == "ShortForm")
            .Value.Value?.ToString();

        return new ConsoloAttributeUsage(names, shortForm);
    }
}