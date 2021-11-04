using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Richiban.Cmdr.Generator
{
    class MethodModelBuilder
    {
        private readonly GeneratorExecutionContext _context;

        private static SymbolDisplayFormat? _symbolDisplayFormat =
            new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle
                    .NameAndContainingTypesAndNamespaces);

        public MethodModelBuilder(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public ImmutableArray<MethodModel> GetMethods(
            CmdrAttributeWriter cmdrAttributeWriter)
        {
            var compilation = _context.Compilation;

            return GetQualifyingMethods(compilation, cmdrAttributeWriter.Name)
                .Choose(m => TryMapMethod(m, compilation, cmdrAttributeWriter.Name))
                .ToImmutableArray();
        }

        private MethodModel? TryMapMethod(
            MethodDeclarationSyntax methodSymtax,
            Compilation compilation,
            string attributeName)
        {
            var semanticModel =
                compilation.GetSemanticModel(methodSymtax.SyntaxTree.GetRoot().SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(methodSymtax) is not { } methodSymbol)
            {
                return null;
            }

            if (!methodSymbol.IsStatic)
            {
                ReportStaticMethodDiagnostic(attributeName, methodSymbol);

                return null;
            }

            var parameters = methodSymbol.Parameters.Select(GetArgumentModel).ToImmutableArray();

            var fullyQualifiedName = GetFullyQualifiedClassName(methodSymbol);

            return new MethodModel(methodSymbol.Name, fullyQualifiedName, parameters);
        }

        private static string GetFullyQualifiedClassName(IMethodSymbol methodSymbol)
        {
            var containingType = methodSymbol.ContainingType;

            return GetFullyQualifiedTypeName(containingType);
        }

        private static string GetFullyQualifiedTypeName(ITypeSymbol containingType) =>
            containingType.ToDisplayString(_symbolDisplayFormat);

        private void ReportStaticMethodDiagnostic(
            string attributeName,
            IMethodSymbol methodSymbol)
        {
            _context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "SG0001",
                        "Non-static method",
                        $"Method {{0}} must be static in order to use the [{attributeName}] attribute",
                        "yeet",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    methodSymbol.Locations.FirstOrDefault(),
                    methodSymbol.Name));
        }

        private IEnumerable<MethodDeclarationSyntax> GetQualifyingMethods(
            Compilation compilation,
            string attributeName)
        {
            bool isQualifying(MethodDeclarationSyntax method)
            {
                return method.AttributeLists.SelectMany(x => x.Attributes)
                    .Any(attr => attr.Name.ToString().Contains(attributeName));
            }

            return compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes())
                .Where(d => d.IsKind(SyntaxKind.MethodDeclaration))
                .OfType<MethodDeclarationSyntax>()
                .Where(isQualifying);
        }

        private static ArgumentModel GetArgumentModel(IParameterSymbol parameterSymbol)
        {
            var name = parameterSymbol.Name;
            var type = GetFullyQualifiedTypeName(parameterSymbol.Type);
            var isFlag = type == "System.Boolean";

            return new ArgumentModel(name, type, isFlag);
        }
    }
}