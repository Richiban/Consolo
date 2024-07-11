using System;
using System.Collections.Generic;

namespace Richiban.Cmdr.Models;

internal record CommandMethod(
        string FullyQualifiedClassName,
        string MethodName,
        IReadOnlyCollection<CommandParameterModel> Parameters)
{
    public string FullyQualifiedName { get; } = $"{FullyQualifiedClassName}.{MethodName}";
}