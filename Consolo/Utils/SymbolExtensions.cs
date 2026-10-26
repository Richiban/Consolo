using System;
using Microsoft.CodeAnalysis;

namespace Consolo;

public static class SymbolExtensions
{
    public static string GetFullyQualifiedName(this ISymbol containingType)
    {
        var fullyQualifiedName = containingType.ToDisplayString(SymbolDisplayFormat);

        return fullyQualifiedName.IndexOf('.') < 0
            ? $"global::{fullyQualifiedName}"
            : fullyQualifiedName;
    }

    private static readonly SymbolDisplayFormat SymbolDisplayFormat = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle
            .NameAndContainingTypesAndNamespaces);
}