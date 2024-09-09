using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Consolo;

static class ParameterModelBuilder
{
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

        if (AttributeUsageUtils.GetUsage(parameterSymbol).IsSome(out var attr))
        {
            if (attr.Name.IsSome(out var givenName))
            {
                if (givenName.Contains(" "))
                {
                    diagnostics.Add(
                        DiagnosticModel.IllegalParameterName(
                            parameterSymbol,
                            givenName
                        )
                    );
                }
                else if (givenName == "")
                {
                    diagnostics.Add(
                        DiagnosticModel.IllegalParameterName(
                            parameterSymbol,
                            givenName
                        )
                    );
                }
                else
                {
                    name = givenName;
                }

                // diagnostics.Add(
                //     DiagnosticModel.AttributeProblem(
                //         ConsoloAttributeDefinition.ShortName,
                //         CandidateReason.MissingName,
                //         Location: parameterSymbol.Locations.FirstOrDefault()
                //     )
                // );
            }

            if (attr.Alias.IsSome(out var givenAlias))
            {
                if (isRequired)
                {
                    diagnostics.Add(
                        DiagnosticModel.AliasOnPositionalParameter(
                            parameterSymbol
                        )
                    );
                }
                else if (givenAlias.Length != 1)
                {
                    diagnostics.Add(
                        DiagnosticModel.AliasMustBeOneCharacter(parameterSymbol)
                    );

                    alias = None;
                }
                else
                {
                    alias = givenAlias;
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
                        Location: parameterSymbol.Locations.FirstOrDefault()
                    ),
                    diagnostics
                )
            );
    }
}
