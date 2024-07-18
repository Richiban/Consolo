using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Richiban.Cmdr;

[Generator]
public class CmdrSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(InjectStaticSourceFiles);

        context.RegisterForSyntaxNotifications(
            () => new CmdrSyntaxReceiver());
    }

    private void InjectStaticSourceFiles(
        GeneratorPostInitializationContext postInitializationContext)
    {
        var cmdrAttributeFileGenerator =
            new CmdrAttributeFileGenerator();

        postInitializationContext.AddSource(
            cmdrAttributeFileGenerator.FileName,
            cmdrAttributeFileGenerator.GetCode());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var diagnosticsManager = new CmdrDiagnosticsManager(context);

        try
        {
            if (context.SyntaxReceiver is not CmdrSyntaxReceiver receiver ||
                receiver.QualifyingMembers.Count == 0)
            {
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
                    $"Found {methodResults.Result.Count} qualifying methods, {methodResults.Diagnostics.Count} diagnostics.",
                    Location: null,
                    Severity: DiagnosticSeverity.Info)
                );

            diagnosticsManager.ReportDiagnostics(methodResults.Diagnostics);

            var rootCommandModel =
                new CommandModelTreeBuilder(diagnosticsManager).Transform(methodResults.Result);

            context.AddCodeFile(new ProgramClassFileGenerator(assemblyName ?? "Unknown assembly", rootCommandModel));
        }
        catch (Exception ex)
        {
            diagnosticsManager.ReportUnknownError(ex);
        }
    }

    private class CmdrSyntaxReceiver : ISyntaxReceiver
    {
        internal List<MethodDeclarationSyntax> QualifyingMembers { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not MethodDeclarationSyntax method)
            {
                return;
            }

            var attribute = method.AttributeLists.SelectMany(
                    list => list.Attributes.Where(x => CmdrAttributeDefinition.Matches(x)))
                .FirstOrDefault();

            if (attribute is null)
            {
                return;
            }

            QualifyingMembers.Add(method);
        }
    }
}
