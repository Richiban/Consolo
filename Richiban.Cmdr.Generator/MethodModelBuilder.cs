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
    class MethodModelBuilder
    {
        private readonly GeneratorExecutionContext _context;
        private readonly CmdrAttribute _cmdrAttribute;

        private static readonly SymbolDisplayFormat SymbolDisplayFormat = new(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle
                .NameAndContainingTypesAndNamespaces);

        public MethodModelBuilder(
            GeneratorExecutionContext context,
            CmdrAttribute cmdrAttribute)
        {
            _context = context;
            _cmdrAttribute = cmdrAttribute;
        }

        public ImmutableArray<MethodModel> GetMethods()
        {
            return GetQualifyingMethods(_context.Compilation)
                .Choose(TryMapMethod)
                .ToImmutableArray();
        }

        private MethodModel? TryMapMethod(IMethodSymbol? methodSymbol)
        {
            if (methodSymbol is null)
            {
                return null;
            }

            if (!methodSymbol.IsStatic)
            {
                ReportStaticMethodDiagnostic(methodSymbol);

                return null;
            }

            var parameters = methodSymbol.Parameters.Select(GetArgumentModel)
                .ToImmutableArray();

            var aliases = GetAttributeArguments(methodSymbol);

            var fullyQualifiedName = GetFullyQualifiedTypeName(methodSymbol.ContainingType);

            return new MethodModel(
                methodSymbol.Name,
                aliases,
                fullyQualifiedName,
                parameters);
        }

        private ImmutableArray<string> GetAttributeArguments(IMethodSymbol methodSymbol)
        {
            return GetAttributesAllTheWayToRoot(methodSymbol)
                .Select(a => a.ConstructorArguments.Single().ToString())
                .ToImmutableArray();
        }

        private Stack<AttributeData> GetAttributesAllTheWayToRoot(
            IMethodSymbol methodSymbol)
        {
            var attributes = new Stack<AttributeData>();

            var current = (ISymbol)methodSymbol;

            while (current != null)
            {
                if (GetAttribute(current) is { } attr)
                    attributes.Push(attr);

                Debugger.Launch();

                current = current.ContainingType;
            }

            return attributes;
        }

        private AttributeData? GetAttribute(ISymbol current)
        {
            return current.GetAttributes()
                .SingleOrDefault(
                    a => a.AttributeClass?.Name == _cmdrAttribute.AttributeName);
        }

        private static string GetFullyQualifiedTypeName(ITypeSymbol containingType) =>
            containingType.ToDisplayString(SymbolDisplayFormat);

        private void ReportStaticMethodDiagnostic(IMethodSymbol methodSymbol)
        {
            _context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "SG0001",
                        "Non-static method",
                        $"Method {{0}} must be static in order to use the [{_cmdrAttribute.AttributeName}] attribute",
                        "yeet",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    methodSymbol.Locations.FirstOrDefault(),
                    methodSymbol.Name));
        }

        private IEnumerable<IMethodSymbol?> GetQualifyingMethods(Compilation compilation)
        {
            bool isCmdrMethodAttribute(AttributeData attr) =>
                attr.AttributeClass?.Name.Contains(_cmdrAttribute.AttributeName) == true;

            bool isQualifying(IMethodSymbol method)
            {
                return method.GetAttributes().Any(isCmdrMethodAttribute);
            }

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

        private static ArgumentModel GetArgumentModel(IParameterSymbol parameterSymbol)
        {
            var name = parameterSymbol.Name;
            var type = GetFullyQualifiedTypeName(parameterSymbol.Type);
            var isFlag = type == "System.Boolean";

            return new ArgumentModel(name, type, isFlag);
        }
    }
}