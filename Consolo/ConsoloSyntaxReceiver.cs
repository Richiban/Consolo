using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Consolo;

internal class ConsoloSyntaxReceiver : ISyntaxReceiver
{
    public MethodDeclarationSyntax? MainMethod { get; set; }
    public GlobalStatementSyntax? FirstGlobalStatement { get; set; }
    internal List<MethodDeclarationSyntax> MarkedMethods { get; } = [];

    internal List<LocalFunctionStatementSyntax> MarkedLocalFunctions { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is GlobalStatementSyntax statement)
        {
            FirstGlobalStatement ??= statement;
            return;
        }

        if (syntaxNode is LocalFunctionStatementSyntax localFunction &&
            localFunction.AttributeLists.SelectMany(x => x.Attributes).Any(ConsoloAttributeDefinition.Matches))
        {
            MarkedLocalFunctions.Add(localFunction);
        }

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