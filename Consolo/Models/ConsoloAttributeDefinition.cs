using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Consolo;

internal static class ConsoloAttributeDefinition
{
    public static string Namespace => "Consolo";
    public static string ShortName => "Consolo";
    public static string LongName => $"{ShortName}Attribute";
    public static string FullyQualifiedName => $"{Namespace}.{LongName}";

    public static bool Matches(INamedTypeSymbol? attrClass) => attrClass?.Name == LongName;

    public static bool Matches(AttributeSyntax attr)
    {
        var attrName = attr.Name.ToString();

        return attrName == LongName || attrName == ShortName;
    }
}