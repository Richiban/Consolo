using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Richiban.Cmdr;

internal class MethodScanner(
    Compilation compilation,
    CmdrDiagnosticsManager diagnostics)
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
                                new DiagnosticModel(
                                    $"There was a problem with attribute {errType.Name}: {candidateReason}",
                                    Location: method.Locations.FirstOrDefault(),
                                    Severity: DiagnosticSeverity.Error));

                            return false;
                        }
                        case var attributeClass:
                            return CmdrAttributeDefinition.Matches(attributeClass);
                    }
                });
}