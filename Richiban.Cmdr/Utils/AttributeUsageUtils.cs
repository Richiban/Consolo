using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Richiban.Cmdr;

public static class AttributeUsageUtils
{
    public static CmdrAttributeUsage? GetUsage(ISymbol symbol)
    {
        var attributeData = symbol.GetAttributes()
            .FirstOrDefault(attr => CmdrAttributeDefinition.Matches(attr.AttributeClass));

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

        return new CmdrAttributeUsage(names, shortForm);
    }
}