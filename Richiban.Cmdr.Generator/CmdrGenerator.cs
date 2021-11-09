using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Richiban.Cmdr.Writers;

namespace Richiban.Cmdr
{
    [Generator]
    public class CmdrGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                if (context.SyntaxReceiver is not MySyntaxReceiver
                    { MethodToAugment: { } method })
                {
                    return;
                }

                var cmdrAttribute = new CmdrAttribute();

                new CmdrAttributeWriter(cmdrAttribute, context).WriteToContext();

                var methods =
                    new MethodModelContextBuilder(context, cmdrAttribute).Build();

                new ProgramClassWriter(context, methods).WriteToContext();

                new ReplWriter(context, cmdrAttribute).WriteToContext();
            }
            catch (Exception ex)
            {
                //Debugger.Launch();

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "Cmdr0004",
                            "Failed!",
                            $"There was a failure: {ex.Message}",
                            "Error",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        location: null));
            }
        }

        class MySyntaxReceiver : ISyntaxReceiver
        {
            public MethodDeclarationSyntax MethodToAugment { get; private set; }

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is MethodDeclarationSyntax method && method.AttributeLists
                    .SelectMany(x => x.Attributes)
                    .Any())
                {
                    MethodToAugment = method;
                }
            }
        }
    }
}