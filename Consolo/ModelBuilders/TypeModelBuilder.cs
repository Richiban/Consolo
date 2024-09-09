using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Consolo;

static class TypeModelBuilder
{
    public static ResultWithDiagnostics<TypeModel> GetTypeModel(ITypeSymbol typeSymbol)
    {
        var diagnostics = new List<DiagnosticModel>();

        return new(
            new TypeModel(
                Name: typeSymbol.Name,
                FullyQualifiedName: typeSymbol.GetFullyQualifiedName(),
                SpecialType: typeSymbol.SpecialType,
                TypeKind: typeSymbol.TypeKind,
                HasParseMethod: typeSymbol.HasParseMethod(),
                HasCastFromString: typeSymbol.HasCastFromString(),
                HasConstructorWithSingleStringParameter: typeSymbol.HasConstructorWithSingleStringParameter(),
                AllowedValues: GetEnumValues(typeSymbol, diagnostics)
            ), 
            diagnostics);
    }


    private static ImmutableArray<TypeValueModel> GetEnumValues(
        ITypeSymbol typeSymbol, List<DiagnosticModel> diagnostics)
    {
        if (typeSymbol.TypeKind != TypeKind.Enum)
        {
            return ImmutableArray<TypeValueModel>.Empty;
        }

        return typeSymbol
            .GetMembers()
            .Where(member => member.Kind == SymbolKind.Field)
            .Select(GetEnumValue)
            .ToImmutableArray();

        TypeValueModel GetEnumValue(ISymbol member)
        {
            var name = member.Name;
            var xmlComments = XmlCommentModelBuilder.GetXmlComments(member);

            diagnostics.AddRange(xmlComments.Diagnostics);

            return new(name, xmlComments.Result.FlatMap(c => c.Summary));
        }
    }
}