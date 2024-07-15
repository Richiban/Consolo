using System.Collections.Generic;

namespace Richiban.Cmdr;

record MethodModel(
    string FullyQualifiedClassName,
    Option<string> Description,
    string MethodName,
    Option<string> ProvidedName,
    IReadOnlyList<CommandPathItem> GroupCommandPath,
    IReadOnlyCollection<ParameterModel> Parameters
);