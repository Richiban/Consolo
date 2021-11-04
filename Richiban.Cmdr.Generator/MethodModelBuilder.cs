using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Richiban.Cmdr.Generator
{
    class MethodModelBuilder
    {
        private readonly GeneratorExecutionContext _context;

        public MethodModelBuilder(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public ImmutableArray<MethodModel> GetMethods(CmdrAttributeWriter c)
        {
            var compilation = _context.Compilation;

            var allMethods = GetQualifyingMethods(compilation);

            return allMethods.Select(m => MapMethod(m, compilation)).ToImmutableArray();
        }

        private static MethodModel MapMethod(
            MethodDeclarationSyntax method,
            Compilation compilation)
        {
            var tree = method.SyntaxTree.GetRoot().SyntaxTree;

            var semanticModel = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            var methodSymbol = semanticModel.GetDeclaredSymbol(
                root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());

            var containingType = methodSymbol.ContainingType;

            var parameters = method.ParameterList.Parameters.Select(GetArgumentModel)
                .ToArray();
            
            var symbolDisplayFormat = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            var fullyQualifiedName = containingType.ToDisplayString(symbolDisplayFormat);

            return new MethodModel(
                methodSymbol.Name,
                fullyQualifiedName,
                parameters);
        }

        private IEnumerable<MethodDeclarationSyntax> GetQualifyingMethods(
            Compilation compilation)
        {
            bool isQualifying(MethodDeclarationSyntax method)
            {
                return method.AttributeLists.SelectMany(x => x.Attributes)
                    .Any(attr => attr.Name.ToString().Contains("CmdrMethod" /*TODO*/));
            }

            return compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes())
                .Where(d => d.IsKind(SyntaxKind.MethodDeclaration))
                .OfType<MethodDeclarationSyntax>()
                .Where(isQualifying);
        }

        private static ArgumentModel GetArgumentModel(ParameterSyntax p)
        {
            var name = p.Identifier.Text;
            var type = p.Type.ToString();
            var isFlag = type.EndsWith("bool") || type.EndsWith("Boolean");

            return new ArgumentModel(name, type, isFlag);
        }
    }
}