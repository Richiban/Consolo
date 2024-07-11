using System.Linq;
using Microsoft.CodeAnalysis;
using Richiban.Cmdr.Models;

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
            .Select(arg => arg.Value?.ToString()!)
            .Where(arg => arg != null)
            .ToArray();

        var description = attributeData.NamedArguments
            .FirstOrDefault(kvp => kvp.Key == "Description")
            .Value.Value?.ToString();

        var shortForm = attributeData.NamedArguments
            .FirstOrDefault(kvp => kvp.Key == "ShortForm")
            .Value.Value?.ToString();

        return new CmdrAttributeUsage(names, description, shortForm);
    }
}