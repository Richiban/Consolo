using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Richiban.Cmdr.Models;

namespace Richiban.Cmdr
{
    internal class CmdrDiagnostics
    {
        private readonly GeneratorExecutionContext _context;

        public CmdrDiagnostics(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public void ReportMethodFailure(MethodModelFailure failure)
        {
            _context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "Cmdr0001",
                        "Failed to register method",
                        failure.Message,
                        "Cmdr",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    failure.Location));
        }

        public void ReportMethodFailures(IEnumerable<MethodModelFailure> failures)
        {
            foreach (var failure in failures)
            {
                ReportMethodFailure(failure);
            }
        }

        public void ReportUnknownError(Exception ex)
        {
            _context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "Cmdr0000",
                        "Unhandled exception",
                        $"There was an unhandled exception ({ex.GetType()}): {ex.Message}, {ex.StackTrace}",
                        "Cmdr",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    location: null));
        }
    }
}