using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Consolo;

internal class ConsoloSyntaxReceiver : ISyntaxReceiver
{
    public MethodDeclarationSyntax? MainMethod { get; set; }
    internal List<MethodDeclarationSyntax> MarkedMethods { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not MethodDeclarationSyntax method)
        {
            return;
        }

        if (method.AttributeLists.SelectMany(x => x.Attributes).Any(ConsoloAttributeDefinition.Matches))
        {
            MarkedMethods.Add(method);
        }

        if (method.Identifier.Text == "Main" && method.Modifiers.Any(mod => mod.IsKind(SyntaxKind.StaticKeyword)))
        {
            MainMethod = method;
        }
    }
}