using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Richiban.Cmdr.Models;

internal class CmdrAttributeDefinition
{
    public string Namespace => "Richiban.Cmdr";
    public string ShortName => "Cmdr";
    public string LongName => $"{ShortName}Attribute";
    public string FullyQualifiedName => $"{Namespace}.{LongName}";

    public bool Matches(INamedTypeSymbol? attrClass) => attrClass?.Name == LongName;

    public bool Matches(AttributeSyntax attr)
    {
        var attrName = attr.Name.ToString();

        return attrName == LongName || attrName == ShortName;
    }
}