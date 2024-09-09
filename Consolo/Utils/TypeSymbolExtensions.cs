using System.Linq;
using Microsoft.CodeAnalysis;

namespace Consolo;

public static class TypeSymbolExtensions
{
    public static bool HasConstructorWithSingleStringParameter(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return false;
        }

        return namedTypeSymbol.Constructors.Any(constructor =>
        {
            var parameters = constructor.Parameters;
            return parameters.Length == 1 && parameters[0].Type.SpecialType == SpecialType.System_String;
        });
    }

    public static bool HasParseMethod(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.GetMembers("Parse").OfType<IMethodSymbol>().Any(method =>
        {
            var parameters = method.Parameters;
            return parameters.Length == 1 && parameters[0].Type.SpecialType == SpecialType.System_String;
        });
    }

    public static bool HasCastFromString(this ITypeSymbol typeSymbol)
    {
        return typeSymbol
            .GetMembers("op_Explicit")
            .Concat(typeSymbol.GetMembers("op_Implicit"))
            .OfType<IMethodSymbol>()
            .Any(method =>
            {
                var parameters = method.Parameters;
                return parameters.Length == 1 && parameters[0].Type.SpecialType == SpecialType.System_String;
            });
    }
}
