using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Consolo;

internal class ConsoloDiagnosticsManager(GeneratorExecutionContext context)
{
    private readonly GeneratorExecutionContext _context = context;

    public void ReportDiagnostic(DiagnosticModel diagnostic)
    {
        _context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    diagnostic.Code,
                    diagnostic.Message,
                    diagnostic.Message,
                    "Consolo",
                    diagnostic.Severity,
                    isEnabledByDefault: true),
                diagnostic.Location));
    }

    public void ReportDiagnostics(IEnumerable<DiagnosticModel> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
        {
            ReportDiagnostic(diagnostic);
        }
    }

    public void ReportUnknownError(Exception ex)
    {
        var interestingStackTrace = ex.StackTrace
            .Split('\n')
            .SkipWhile(line => !line.Contains("Consolo"))
            .TakeWhile(line => line.Contains("Consolo"))
            .StringJoin("\n");

        _context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "Consolo0000",
                    "Unhandled exception",
                    $"There was an unhandled exception ({ex.GetType()}): {ex.Message}, {interestingStackTrace}",
                    "Consolo",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                location: null));
    }
}