using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;

namespace Consolo;

static class ParameterModelBuilder
{
    private static readonly Regex parameterNameSpec =
        new(pattern: "^[a-z]([-a-zA-Z0-9]*)$",
            options: RegexOptions.Compiled);

    public static ResultWithDiagnostics<ParameterModel> GetParameterModel(
        IParameterSymbol parameterSymbol,
        Option<XmlCommentModel> methodXmlComments)
    {
        var diagnostics = new List<DiagnosticModel>();

        var paramsElementType = GetParamsElementType(parameterSymbol);
        var type = parameterSymbol.Type.GetFullyQualifiedName();
        var isFlag = type == "System.Boolean";
        var name = isFlag
            ? StringUtils.ToKebabCase(parameterSymbol.Name)
            : parameterSymbol.Name;
        var alias = (Option<string>)None;
        var isRequired = !parameterSymbol.HasExplicitDefaultValue;
        var defaultValue =
            parameterSymbol.HasExplicitDefaultValue
            ? SourceValueUtils.SourceValue(parameterSymbol.ExplicitDefaultValue, parameterSymbol.Type)
            : null;
        var helpDefaultValue =
            parameterSymbol.HasExplicitDefaultValue
                ? GetHelpDefaultValue(parameterSymbol)
                : null;
        var xmlComment = methodXmlComments.FlatMap(x => x[parameterSymbol.Name]);
        var syntax = parameterSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        var nameLocation = (Option<Location>)None;
        var aliasLocation = (Option<Location>)None;

        if (AttributeUsageUtils.GetUsage(parameterSymbol).IsSome(out var attr))
        {
            if (attr.Name.IsSome(out var givenName))
            {
                if (parameterNameSpec.IsMatch(givenName))
                {
                    name = givenName;
                    nameLocation = attr.NameLocation;
                }
                else
                {
                    diagnostics.Add(
                        DiagnosticModel.IllegalParameterName(
                            attr.NameLocation | attr.AttributeLocation)
                    );
                }
            }

            if (attr.Alias.IsSome(out var givenAlias))
            {
                if (isRequired)
                {
                    diagnostics.Add(
                        DiagnosticModel.AliasOnPositionalParameter(attr.AliasLocation | attr.AttributeLocation)
                    );
                }
                else if (givenAlias.Length != 1 || givenAlias[0] is not (>= 'a' and <= 'z'))
                {
                    diagnostics.Add(
                        DiagnosticModel.IllegalAlias(attr.AliasLocation | attr.AttributeLocation)
                    );

                    alias = None;
                }
                else
                {
                    alias = givenAlias;
                    aliasLocation = attr.AliasLocation;
                }
            }
        }

        return TypeModelBuilder
            .GetTypeModel(paramsElementType ?? parameterSymbol.Type)
            .FlatMap(parameterType =>
                new ResultWithDiagnostics<ParameterModel>(
                    new ParameterModel(
                        Name: name,
                        SourceName: parameterSymbol.Name,
                        IsFlag: isFlag,
                        IsRequired: isRequired,
                        DefaultValue: defaultValue,
                        HelpDefaultValue: helpDefaultValue,
                        Description: xmlComment,
                        Alias: alias,
                        Type: parameterType,
                        Location: parameterSymbol.Locations.FirstOrDefault(),
                        NameLocation: nameLocation,
                        AliasLocation: aliasLocation,
                        Params: paramsElementType is null
                            ? ParamsKind.None
                            : parameterSymbol.Type is IArrayTypeSymbol
                                ? ParamsKind.Array
                                : ParamsKind.Collection,
                        ParamsTypeName: parameterSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    ),
                    diagnostics
                )
            );
    }

    // Covers params arrays and C# 13 params collections. A params collection's element type is its iteration type,
    // found the way foreach finds it: the public GetEnumerator pattern first, then IEnumerable<T>
    private static ITypeSymbol? GetParamsElementType(IParameterSymbol parameterSymbol)
    {
        if (!parameterSymbol.IsParams)
        {
            return null;
        }

        return parameterSymbol.Type switch
        {
            IArrayTypeSymbol array => array.ElementType,
            INamedTypeSymbol { TypeKind: TypeKind.Interface } named => GetEnumerableOfTElementType(named),
            INamedTypeSymbol named => GetPatternIterationType(named) ?? GetEnumerableOfTElementType(named),
            _ => null
        };
    }

    private static ITypeSymbol? GetEnumerableOfTElementType(INamedTypeSymbol type) =>
        new[] { type }
            .Concat(type.AllInterfaces)
            .FirstOrDefault(t => t.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            ?.TypeArguments[0];

    private static ITypeSymbol? GetPatternIterationType(INamedTypeSymbol type)
    {
        var getEnumerator = GetTypeAndBaseTypes(type)
            .SelectMany(t => t.GetMembers("GetEnumerator"))
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m is { IsStatic: false, Parameters.Length: 0, DeclaredAccessibility: Accessibility.Public });

        return getEnumerator?.ReturnType switch
        {
            // e.g. a GetEnumerator() that returns IEnumerator<T>
            INamedTypeSymbol { TypeKind: TypeKind.Interface } enumeratorInterface =>
                new[] { enumeratorInterface }
                    .Concat(enumeratorInterface.AllInterfaces)
                    .FirstOrDefault(t => t.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerator_T)
                    ?.TypeArguments[0],
            INamedTypeSymbol enumeratorType =>
                GetTypeAndBaseTypes(enumeratorType)
                    .SelectMany(t => t.GetMembers("Current"))
                    .OfType<IPropertySymbol>()
                    .FirstOrDefault(p => p is { IsStatic: false, DeclaredAccessibility: Accessibility.Public })
                    ?.Type,
            _ => null
        };
    }

    private static IEnumerable<INamedTypeSymbol> GetTypeAndBaseTypes(INamedTypeSymbol type)
    {
        for (var t = type; t is not null; t = t.BaseType)
        {
            yield return t;
        }
    }

    private static string GetHelpDefaultValue(IParameterSymbol parameterSymbol)
    {
        var value = parameterSymbol.ExplicitDefaultValue;

        if (value is null)
        {
            return "null";
        }

        if (parameterSymbol.Type.TypeKind == TypeKind.Enum)
        {
            var enumMember = parameterSymbol.Type
                .GetMembers()
                .OfType<IFieldSymbol>()
                .FirstOrDefault(member =>
                    member.HasConstantValue && Equals(member.ConstantValue, value));

            if (enumMember is not null)
            {
                return StringUtils.ToCamelCase(enumMember.Name);
            }
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
    }
}
