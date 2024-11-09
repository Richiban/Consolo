using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace Consolo;

internal static class ConsoloSourceGenerator2
{
    public static Option<CommandTree.Root> GenerateCommandTree(
        GeneratorExecutionContext context,
        ConsoloDiagnosticsManager diagnosticsManager, 
        ConsoloSyntaxReceiver receiver)
    {
        if (receiver.MainMethod is {} mainMethod)
        {
            diagnosticsManager.ReportDiagnostic(DiagnosticModel.MainMethodFound(
                mainMethod.Identifier.GetLocation()));
        }
        
        if (receiver.MarkedMethods.Count == 0)
        {
            diagnosticsManager.ReportDiagnostic(DiagnosticModel.NoMethodsFound());

            return None;
        }

        var methodResults = BuildMethods(context, receiver.MarkedMethods);

        if (methodResults.Result.Count == 0 && methodResults.Diagnostics.Count == 0)
        {
            diagnosticsManager.ReportDiagnostic(DiagnosticModel.NoMethodsFound());

            return None;
        }

        diagnosticsManager.ReportDiagnostic(DiagnosticModel.MethodScanInfo(methodResults));

        diagnosticsManager.ReportDiagnostics(methodResults.Diagnostics);

        var (rootCommandModel, transformingDiagnostics) = CommandTreeBuilder.Transform(methodResults.Result);

        VersionCommandAdder.AddVersionCommand(rootCommandModel);

        diagnosticsManager.ReportDiagnostics(transformingDiagnostics);

        return rootCommandModel;
    }

    private static ResultWithDiagnostics<IReadOnlyCollection<MethodModel>> BuildMethods(
        GeneratorExecutionContext context,
        IReadOnlyCollection<MethodDeclarationSyntax> markedMethods)
    {
        var candidateMethods = markedMethods.Select(GetMethodSymbol).Where(symbol => symbol != null)!;

        return MethodModelBuilder.BuildFrom(candidateMethods);

        IMethodSymbol? GetMethodSymbol(MethodDeclarationSyntax method) =>
            CSharpExtensions.GetDeclaredSymbol(context.Compilation.GetSemanticModel(method.SyntaxTree), method);
    }
}