using System.Linq;
using Microsoft.CodeAnalysis;

namespace Consolo;

static class AttributeUsageUtils
{
    public static Option<ConsoloAttributeUsage> GetUsage(ISymbol symbol)
    {
        var attributeData = symbol.GetAttributes()
            .FirstOrDefault(attr => ConsoloAttributeDefinition.Matches(attr.AttributeClass));

        if (attributeData is null)
        {
            return null;
        }

        var name = attributeData.ConstructorArguments
            .Select(arg => arg.Value?.ToString())
            .SingleOrDefault();

        var alias = attributeData.NamedArguments
            .FirstOrDefault(kvp => kvp.Key == "Alias")
            .Value.Value?.ToString();

        return new ConsoloAttributeUsage(name, alias);
    }
}