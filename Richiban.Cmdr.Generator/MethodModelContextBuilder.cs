using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Richiban.Cmdr.Models;

namespace Richiban.Cmdr
{
    internal class MethodModelContextBuilder
    {
        private readonly GeneratorExecutionContext _context;
        private readonly MethodModelBuilder _builder;
        private readonly CmdrAttribute _cmdrAttribute;

        public MethodModelContextBuilder(
            GeneratorExecutionContext context,
            CmdrAttribute cmdrAttribute)
        {
            _context = context;
            _cmdrAttribute = cmdrAttribute;
            _builder = new MethodModelBuilder(cmdrAttribute);
        }

        public ImmutableArray<MethodModel> Build()
        {
            var qualifyingMethods = GetQualifyingMethods(_context.Compilation);

            var (methodModels, failures) = _builder.BuildFrom(qualifyingMethods).Partition();

            RegisterFailures(failures);

            return methodModels;
        }

        private void RegisterFailures(IEnumerable<MethodModelFailure> failures)
        {
            foreach (var failure in failures)
            {
                ReportMethodFailureDiagnostic(failure);
            }
        }

        private IEnumerable<IMethodSymbol?> GetQualifyingMethods(Compilation compilation)
        {
            bool isCmdrMethodAttribute(AttributeData attr) =>
                attr.AttributeClass?.Name.Contains(_cmdrAttribute.AttributeName) == true;

            bool isQualifying(IMethodSymbol method) =>
                method.GetAttributes().Any(isCmdrMethodAttribute);

            IEnumerable<IMethodSymbol> getMethodSymbols(
                IEnumerable<MethodDeclarationSyntax> methodDeclarations,
                SemanticModel semanticModel)
            {
                return methodDeclarations.Choose(
                    methodDeclaration =>
                        semanticModel.GetDeclaredSymbol(methodDeclaration));
            }

            (IEnumerable<MethodDeclarationSyntax> methodDeclarations, SemanticModel
                semanticModel) getMethodDeclarationsAndModel(SyntaxTree tree)
            {
                var methodDeclarations = tree.GetRoot()
                    .DescendantNodes()
                    .Where(d => d.IsKind(SyntaxKind.MethodDeclaration))
                    .OfType<MethodDeclarationSyntax>();

                var semanticModel = compilation.GetSemanticModel(tree);

                return (methodDeclarations, semanticModel);
            }

            return compilation.SyntaxTrees.Select(getMethodDeclarationsAndModel)
                .SelectMany(
                    pair => getMethodSymbols(pair.methodDeclarations, pair.semanticModel))
                .Where(isQualifying);
        }

        private void ReportMethodFailureDiagnostic(MethodModelFailure failure)
        {
            _context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "Cmdr0001",
                        "Failed to register method",
                        failure.Message,
                        "Cmdr",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    failure.Location));
        }
    }
}