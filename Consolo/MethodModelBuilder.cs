using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Consolo;

internal class MethodModelBuilder
{
    public ResultWithDiagnostics<IReadOnlyCollection<MethodModel>> BuildFrom(
        IEnumerable<IMethodSymbol?> qualifyingMethods) => qualifyingMethods
            .Select(TryMapMethod)
            .CollectResults();

    private ResultWithDiagnostics<Option<MethodModel>> TryMapMethod(
        IMethodSymbol? methodSymbol)
    {
        if (methodSymbol is null)
        {
            return ResultWithDiagnostics.DiagnosticOnly<MethodModel>(
                DiagnosticModel.ErrorProcessingMethod("Method not found",
                Location: null));
        }

        if (!methodSymbol.IsStatic)
        {
            return ResultWithDiagnostics.DiagnosticOnly<MethodModel>(
                DiagnosticModel.MethodMustBeStatic(methodSymbol));
        }

        var xmlComments = XmlCommentModelBuilder.GetXmlComments(methodSymbol);

        var parameterResults = methodSymbol.Parameters
            .Select(p => GetArgumentModel(p, xmlComments.Result))
            .CollectResults();

        var fullyQualifiedName = methodSymbol.ContainingType.GetFullyQualifiedName();

        var commandPath = GetCommandPath(methodSymbol);
        var parentNames = commandPath.Truncate(count: -1).ToList();
        var lastPathItem = commandPath.LastOrDefault();

        // TODO remove this if nothing else added
        List<DiagnosticModel> diagnostics = [
            ..parameterResults.Diagnostics
        ];

        return new ResultWithDiagnostics<Option<MethodModel>>(
            new MethodModel(
                MethodName: methodSymbol.Name,
                ProvidedName: lastPathItem.Name,
                FullyQualifiedClassName: fullyQualifiedName,
                ParentCommandPath: parentNames,
                Parameters: parameterResults.Result,
                Description: xmlComments.Result.FlatMap(x => x.Summary),
                Location: methodSymbol.Locations.FirstOrDefault()
            ),
            diagnostics);
    }

    private ImmutableList<CommandPathItem> GetCommandPath(ISymbol symbol)
    {
        var path = ImmutableList.CreateBuilder<CommandPathItem>();

        while (symbol != null)
        {
            if (AttributeUsageUtils.GetUsage(symbol).IsSome(out var attr))
            {
                var commandName = attr.Name | symbol.Name;
                var xmlComment = XmlCommentModelBuilder.GetXmlComments(symbol)
                    .Result.FlatMap(r => r.Summary);

                path.Add(new(commandName, xmlComment));
            }

            symbol = symbol.ContainingType;
        }

        path.Reverse();

        return path.ToImmutable();
    }

    private ResultWithDiagnostics<ParameterModel> GetArgumentModel(
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

        return new ResultWithDiagnostics<ParameterModel>(
            new ParameterModel(
                Name: name,
                OriginalName: parameterSymbol.Name,
                IsFlag: isFlag,
                IsRequired: isRequired,
                DefaultValue: defaultValue,
                Description: xmlComment,
                Alias: alias,
                Type: parameterSymbol.Type,
                Location: parameterSymbol.Locations.FirstOrDefault()),
            diagnostics
        );
    }
}
