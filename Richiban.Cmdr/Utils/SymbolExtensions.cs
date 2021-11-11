using System;
using Microsoft.CodeAnalysis;

namespace Richiban.Cmdr.Utils
{
    public static class SymbolExtensions
    {
        public static string GetFullyQualifiedName(this ISymbol containingType) =>
            containingType.ToDisplayString(SymbolDisplayFormat);

        private static readonly SymbolDisplayFormat SymbolDisplayFormat = new(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle
                .NameAndContainingTypesAndNamespaces);
    }
}