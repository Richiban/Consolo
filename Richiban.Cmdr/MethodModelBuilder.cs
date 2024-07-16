using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Richiban.Cmdr;

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
                new DiagnosticModel("Method not found",
                Location: null,
                Severity: DiagnosticSeverity.Error));
        }

        if (!methodSymbol.IsStatic)
        {
            return ResultWithDiagnostics.DiagnosticOnly<MethodModel>(
                new DiagnosticModel(
                    $"Method {methodSymbol} must be static in order to use the {CmdrAttributeDefinition.ShortName} attribute.",
                    methodSymbol.Locations.FirstOrDefault(),
                    Severity: DiagnosticSeverity.Error));
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
                GroupCommandPath: parentNames,
                Parameters: parameterResults.Result,
                Description: xmlComments.Result.FlatMap(x => x.Summary)
            ),
            diagnostics);
    }

    private ImmutableList<CommandPathItem> GetCommandPath(ISymbol symbol)
    {
        var path = ImmutableList.CreateBuilder<CommandPathItem>();

        while (symbol != null)
        {
            if (AttributeUsageUtils.GetUsage(symbol) is { } attr)
            {
                var commandName = attr.Names.LastOrDefault() ?? symbol.Name;
                var xmlComment = XmlCommentModelBuilder.GetXmlComments(symbol)
                    .Result.FlatMap(r => r.Summary);

                if (commandName == "" && path is { Count: > 0 }) 
                {
                    var parent = path.Last();
                    path.RemoveAt(path.Count - 1);
                    path.Add(new CommandPathItem(parent.Name, xmlComment));
                }
                else
                {
                    path.Add(new(commandName, xmlComment));
                }

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
        var attr = AttributeUsageUtils.GetUsage(parameterSymbol);
        var diagnostics = new List<DiagnosticModel>();

        if (attr is {Names: { Count: > 1 and var count }})
        {
            diagnostics.Add(
                new DiagnosticModel(
                    $"Parameter {parameterSymbol.Name} has {count} names specified in the attribute; parameters can only specify one.",
                    Location: parameterSymbol.Locations.FirstOrDefault(),
                    Severity: DiagnosticSeverity.Error));
        }

        var name = attr?.Names?.FirstOrDefault() ?? parameterSymbol.Name;

        var type = parameterSymbol.Type.GetFullyQualifiedName();
        var isFlag = type == "System.Boolean";
        var isRequired = !parameterSymbol.HasExplicitDefaultValue;
        var defaultValue =
            parameterSymbol.HasExplicitDefaultValue
            ? SourceValueUtils.SourceValue(parameterSymbol.ExplicitDefaultValue)
            : null;
        var xmlComment = methodXmlComments.FlatMap(x => x[name]);
        var shortForm = attr?.ShortForm;

        return new ResultWithDiagnostics<ParameterModel>(
            new ParameterModel(
                name,
                type,
                isFlag,
                IsRequired: isRequired,
                DefaultValue: defaultValue,
                Description: xmlComment,
                ShortForm: shortForm,
                type: parameterSymbol.Type),
            diagnostics
        );
    }
}
