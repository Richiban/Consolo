using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Richiban.Cmdr.Models;

namespace Richiban.Cmdr
{
    internal class MethodScanner
    {
        private readonly CmdrAttributeDefinition _cmdrAttributeDefinition;
        private readonly Compilation _compilation;
        private readonly CmdrDiagnostics _diagnostics;

        public MethodScanner(
            Compilation compilation,
            CmdrAttributeDefinition cmdrAttributeDefinition,
            CmdrDiagnostics diagnostics)
        {
            _compilation = compilation;
            _cmdrAttributeDefinition = cmdrAttributeDefinition;
            _diagnostics = diagnostics;
        }

        public IEnumerable<IMethodSymbol> GetCandidateMethods(
            IEnumerable<MethodDeclarationSyntax> methodDeclarations)
        {
            return methodDeclarations.Select(
                    method =>
                    {
                        var semanticModel =
                            _compilation.GetSemanticModel(method.SyntaxTree);

                        return (method, semanticModel);
                    })
                .SelectNotNull(pair => pair.semanticModel.GetDeclaredSymbol(pair.method))
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
                                var candidateSymbols = errType.CandidateSymbols;

                                _diagnostics.ReportMethodFailure(
                                    new MethodModelFailure(
                                        $"There was a problem with attribute {errType.Name}: {candidateReason}",
                                        location: null));

                                return false;
                            }
                            case var attributeClass:
                                return _cmdrAttributeDefinition.Matches(attributeClass);
                        }
                    });
    }
}