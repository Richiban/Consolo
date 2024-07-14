using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Richiban.Cmdr;

internal class CmdrDiagnosticsManager(GeneratorExecutionContext context)
{
    private readonly GeneratorExecutionContext _context = context;

    public void ReportDiagnostic(DiagnosticModel diagnostic)
    {
        _context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "Cmdr0001",
                    diagnostic.Message,
                    diagnostic.Message,
                    "Cmdr",
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
            .SkipWhile(line => !line.Contains("Richiban.Cmdr"))
            .TakeWhile(line => line.Contains("Richiban.Cmdr"))
            .StringJoin("\n");

        _context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "Cmdr0000",
                    "Unhandled exception",
                    $"There was an unhandled exception ({ex.GetType()}): {ex.Message}, {interestingStackTrace}",
                    "Cmdr",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                location: null));
    }
}