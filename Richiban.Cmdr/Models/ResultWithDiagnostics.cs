using System.Collections.Generic;

namespace Richiban.Cmdr;

class ResultWithDiagnostics<T>
{
    public T Result { get; }
    public IReadOnlyCollection<DiagnosticModel> Diagnostics { get; }

    public ResultWithDiagnostics(T result, IReadOnlyCollection<DiagnosticModel> diagnostics)
    {
        Result = result;
        Diagnostics = diagnostics;
    }
    
    public static implicit operator ResultWithDiagnostics<T>(T result)
    {
        return new ResultWithDiagnostics<T>(result, []);
    }
}

static class ResultWithDiagnostics
{
    internal static ResultWithDiagnostics<Option<T>> DiagnosticOnly<T>(
        params DiagnosticModel[] diagnostics) where T : class
    {
        return new ResultWithDiagnostics<Option<T>>(new(), diagnostics);
    }
}