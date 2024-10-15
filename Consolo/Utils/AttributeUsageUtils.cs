using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        var attributeSyntax = attributeData.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax;

        var attributeLocation = attributeSyntax?.GetLocation();
        var nameLocation = attributeSyntax?.ArgumentList?.Arguments.FirstOrDefault(arg => arg.NameEquals is null)?.GetLocation();
        var aliasLocation = attributeSyntax?.ArgumentList?.Arguments.FirstOrDefault(arg => arg.NameEquals is not null)?.GetLocation();

        return new ConsoloAttributeUsage(name, alias, attributeLocation, nameLocation, aliasLocation);
    }
}