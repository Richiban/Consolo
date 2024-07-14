using System.Collections.Generic;

namespace Richiban.Cmdr;

record MethodModel
{
    public string FullyQualifiedClassName { get; init; }
    public Option<string> Description { get; init; }
    public string MethodName { get; init; }
    public Option<string> ProvidedName { get; init; }
    public IReadOnlyList<CommandPathItem> GroupCommandPath { get; init; }
    public IReadOnlyCollection<ParameterModel> Parameters { get; init; }
}
