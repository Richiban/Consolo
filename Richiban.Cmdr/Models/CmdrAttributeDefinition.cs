using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Richiban.Cmdr.Models;

internal static class CmdrAttributeDefinition
{
    public static string Namespace => "Richiban.Cmdr";
    public static string ShortName => "Cmdr";
    public static string LongName => $"{ShortName}Attribute";
    public static string FullyQualifiedName => $"{Namespace}.{LongName}";

    public static bool Matches(INamedTypeSymbol? attrClass) => attrClass?.Name == LongName;

    public static bool Matches(AttributeSyntax attr)
    {
        var attrName = attr.Name.ToString();

        return attrName == LongName || attrName == ShortName;
    }
}