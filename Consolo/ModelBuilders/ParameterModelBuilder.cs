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

        var collectionElementType = GetCollectionElementType(parameterSymbol.Type);
        var isCollection = collectionElementType is not null;
        var type = parameterSymbol.Type.GetFullyQualifiedName();
        var isFlag = type == "System.Boolean";
        var name = isFlag
            ? StringUtils.ToKebabCase(parameterSymbol.Name)
            : parameterSymbol.Name;
        var alias = (Option<string>)None;
        var isRequired = !parameterSymbol.HasExplicitDefaultValue;
        var defaultValue =
            parameterSymbol.HasExplicitDefaultValue && !isCollection
            ? SourceValueUtils.SourceValue(parameterSymbol.ExplicitDefaultValue, parameterSymbol.Type)
            : null;
        var helpDefaultValue =
            parameterSymbol.HasExplicitDefaultValue && !isCollection
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
            .GetTypeModel(collectionElementType ?? parameterSymbol.Type)
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
                        IsCollection: isCollection,
                        IsParams: isCollection && parameterSymbol.IsParams
                    ),
                    diagnostics
                )
            );
    }

    private static ITypeSymbol? GetCollectionElementType(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol { Rank: 1 } array)
        {
            return array.ElementType;
        }

        if (type is INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } named
            && named.OriginalDefinition.SpecialType is
                SpecialType.System_Collections_Generic_IEnumerable_T
                or SpecialType.System_Collections_Generic_IReadOnlyCollection_T
                or SpecialType.System_Collections_Generic_IReadOnlyList_T
                or SpecialType.System_Collections_Generic_ICollection_T
                or SpecialType.System_Collections_Generic_IList_T)
        {
            return named.TypeArguments[0];
        }

        return null;
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
