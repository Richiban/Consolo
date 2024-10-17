using System.Collections.Generic;
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

        var name = parameterSymbol.Name;
        var alias = (Option<string>)None;
        var type = parameterSymbol.Type.GetFullyQualifiedName();
        var isFlag = type == "System.Boolean";
        var isRequired = !parameterSymbol.HasExplicitDefaultValue;
        var defaultValue =
            parameterSymbol.HasExplicitDefaultValue
            ? SourceValueUtils.SourceValue(parameterSymbol.ExplicitDefaultValue)
            : null;
        var xmlComment = methodXmlComments.FlatMap(x => x[parameterSymbol.Name]);
        var syntax = parameterSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        var nameLocation = (Option<Location>)None;
        var aliasLocation = (Option<Location>)None;

        if (AttributeUsageUtils.GetUsage(parameterSymbol).IsSome(out var attr))
        {
            if (attr.Name.IsSome(out var givenName))
            {
                if (!parameterNameSpec.IsMatch(givenName))
                {
                    diagnostics.Add(
                        DiagnosticModel.IllegalParameterName(attr.NameLocation | attr.AttributeLocation)
                    );
                }
                else
                {
                    name = givenName;
                    nameLocation = attr.NameLocation;
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
            .GetTypeModel(parameterSymbol.Type)
            .FlatMap(parameterType =>
                new ResultWithDiagnostics<ParameterModel>(
                    new ParameterModel(
                        Name: name,
                        SourceName: parameterSymbol.Name,
                        IsFlag: isFlag,
                        IsRequired: isRequired,
                        DefaultValue: defaultValue,
                        Description: xmlComment,
                        Alias: alias,
                        Type: parameterType,
                        Location: parameterSymbol.Locations.FirstOrDefault(),
                        NameLocation: nameLocation,
                        AliasLocation: aliasLocation
                    ),
                    diagnostics
                )
            );
    }
}
