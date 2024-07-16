using System;

namespace Richiban.Cmdr;

static class ResultWithDiagnostics
{
    internal static ResultWithDiagnostics<Option<T>> DiagnosticOnly<T>(
        params DiagnosticModel[] diagnostics) where T : class
    {
        return new ResultWithDiagnostics<Option<T>>(new(), diagnostics);
    }
}