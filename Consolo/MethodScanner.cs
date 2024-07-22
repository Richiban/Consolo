using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Consolo;

internal class MethodScanner(
    Compilation compilation,
    ConsoloDiagnosticsManager diagnostics)
{
    public IEnumerable<IMethodSymbol> GetCandidateMethods(
        IEnumerable<MethodDeclarationSyntax> methodDeclarations)
    {
        return methodDeclarations.Select(
                method =>
                {
                    var semanticModel =
                        compilation.GetSemanticModel(method.SyntaxTree);

                    return (method, semanticModel);
                })
            .Select(pair => pair.semanticModel.GetDeclaredSymbol(pair.method)!)
            .Where(it => it is not null)
            .Where(IsQualifying);
    }

    private bool IsQualifying(ISymbol method) =>
        method.GetAttributes()
            .Any(
                attr =>
                {
                    switch (attr.AttributeClass)
                    {
                        case null: return false;
                        case IErrorTypeSymbol errType:
                        {
                            var candidateReason = errType.CandidateReason;

                            diagnostics.ReportDiagnostic(
                                DiagnosticModel.AttributeProblem(
                                    errType.Name,
                                    candidateReason,
                                    Location: method.Locations.FirstOrDefault()));

                            return false;
                        }
                        case var attributeClass:
                            return ConsoloAttributeDefinition.Matches(attributeClass);
                    }
                });
}