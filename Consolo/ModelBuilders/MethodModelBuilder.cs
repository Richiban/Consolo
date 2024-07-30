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
            .Select(p => ParameterModelBuilder.GetParameterModel(p, xmlComments.Result))
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

    
}
