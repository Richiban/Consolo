using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Consolo;

record MethodModel(
    string FullyQualifiedClassName,
    Option<string> Description,
    string MethodName,
    Option<string> ProvidedName,
    IReadOnlyList<CommandPathItem> ParentCommandPath,
    IReadOnlyCollection<ParameterModel> Parameters,
    Option<Location> Location,
    ITypeSymbol ReturnType
);