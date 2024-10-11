using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Consolo;

[Generator]
public class ConsoloSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(InjectStaticSourceFiles);

        context.RegisterForSyntaxNotifications(
            () => new ConsoloSyntaxReceiver());
    }

    private void InjectStaticSourceFiles(
        GeneratorPostInitializationContext postInitializationContext)
    {
        var ConsoloAttributeFileGenerator =
            new ConsoloAttributeFileGenerator();

        postInitializationContext.AddSource(
            ConsoloAttributeFileGenerator.FileName,
            ConsoloAttributeFileGenerator.GetCode());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var diagnosticsManager = new ConsoloDiagnosticsManager(context);

        try
        {
            if (context.SyntaxReceiver is not ConsoloSyntaxReceiver receiver)
            {
                return;
            }

            if (receiver.QualifyingMembers.Count == 0)
            {
                diagnosticsManager.ReportDiagnostic(DiagnosticModel.NoMethodsFound());

                return;
            }

            var assemblyName = context.Compilation.AssemblyName;

            var candidateMethods =
                new MethodScanner(context.Compilation, diagnosticsManager)
                    .GetCandidateMethods(receiver.QualifyingMembers);

            var methodResults = new MethodModelBuilder()
                .BuildFrom(candidateMethods);

            diagnosticsManager.ReportDiagnostic(
                new DiagnosticModel(
                    Code: "Consolo0000",
                    $"Found {methodResults.Result.Count} qualifying methods, {methodResults.Diagnostics.Count} diagnostics.",
                    Location: null,
                    Severity: DiagnosticSeverity.Info)
                );

            if (methodResults.Result.Count == 0 && methodResults.Diagnostics.Count == 0)
            {
                diagnosticsManager.ReportDiagnostic(DiagnosticModel.NoMethodsFound());

                return;
            }

            diagnosticsManager.ReportDiagnostics(methodResults.Diagnostics);

            var (rootCommandModel, transformingDiagnostics) =
                new CommandTreeBuilder().Transform(methodResults.Result);

            diagnosticsManager.ReportDiagnostics(transformingDiagnostics);

            context.AddCodeFile(
                new ProgramClassFileGenerator(
                    assemblyName ?? "Unknown assembly",
                    rootCommandModel
                )
            );
        }
        catch (Exception ex)
        {
            diagnosticsManager.ReportUnknownError(ex);
        }
    }

    private class ConsoloSyntaxReceiver : ISyntaxReceiver
    {
        internal List<MethodDeclarationSyntax> QualifyingMembers { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not MethodDeclarationSyntax method)
            {
                return;
            }

            var attribute = method.AttributeLists.SelectMany(
                    list => list.Attributes.Where(x => ConsoloAttributeDefinition.Matches(x)))
                .FirstOrDefault();

            if (attribute is null)
            {
                return;
            }

            QualifyingMembers.Add(method);
        }
    }
}
