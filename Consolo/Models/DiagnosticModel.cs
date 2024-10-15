using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Consolo;

record DiagnosticModel(string Code, string Message, Location? Location, DiagnosticSeverity Severity)
{
    public static DiagnosticModel NoMethodsFound() =>
        new DiagnosticModel(
            Code: "Consolo0002",
            Message: $"No command methods found. Make sure you're using the {ConsoloAttributeDefinition.ShortName} attribute.",
            Location: null,
            DiagnosticSeverity.Error);

    public static DiagnosticModel DuplicateCommand(string commandName, Option<Location> location) =>
        new DiagnosticModel(
            Code: "Consolo0003",
            Message: $"The command name '{commandName}' is already in use.",
            Location: location | null!,
            DiagnosticSeverity.Error);

    public static DiagnosticModel DuplicateCommand(
        string commandName,
        string parentCommandName, 
        Option<Location> location) =>
        new DiagnosticModel(
            Code: "Consolo0003",
            Message: $"The command name '{commandName}' is already in group '{parentCommandName}'.",
            Location: location | null!,
            DiagnosticSeverity.Error);

    internal static DiagnosticModel ErrorProcessingMethod(
        string message,
        Location? Location) =>
        new DiagnosticModel(
            Code: "Consolo0004",
            Message: message,
            Location: Location,
            Severity: DiagnosticSeverity.Error
        );

    internal static DiagnosticModel MethodMustBeStatic(IMethodSymbol methodSymbol) =>
        new DiagnosticModel(
            Code: "Consolo0005",
            Message: $"Method {methodSymbol} must be static in order to use the {ConsoloAttributeDefinition.ShortName} attribute.",
            Location: methodSymbol.Locations.FirstOrDefault(),
            Severity: DiagnosticSeverity.Error
        );

    internal static DiagnosticModel AttributeProblem(
        string errTypeName,
        CandidateReason candidateReason,
        Location? Location) =>
        new DiagnosticModel(
            Code: "Consolo0006",
            Message: $"There was a problem with attribute {errTypeName}: {candidateReason}",
            Location: Location,
            Severity: DiagnosticSeverity.Error
        );

    internal static DiagnosticModel IllegalParameterName(Option<Location> nameLocation) =>
        new DiagnosticModel(
            Code: "Consolo0007",
            Message: $"Parameter names must have the form [a-z][-a-zA-Z]*.",
            Location: nameLocation | null!,
            Severity: DiagnosticSeverity.Error
        );

    internal static DiagnosticModel UnsupportedParameterType(ParameterModel param) => 
        new DiagnosticModel(
            Code: "Consolo0008",
            Message: $"Parameter '{param.Name}' has a type that is unsupported ({param.Type.Name}).",
            Location: param.Location | null!,
            Severity: DiagnosticSeverity.Error
        );

    internal static DiagnosticModel IllegalAlias(Option<Location> aliasLocation) =>
        new DiagnosticModel(
            Code: "Consolo0009",
            Message: $"Parameter aliases must be a single character in the range [a-z].",
            Location: aliasLocation | null!,
            Severity: DiagnosticSeverity.Error
        );

    internal static DiagnosticModel AliasOnPositionalParameter(Option<Location> aliasLocation) =>
        new DiagnosticModel(
            Code: "Consolo0010",
            Message: $"Positional parameters cannot have an alias.",
            Location: aliasLocation | null!,
            Severity: DiagnosticSeverity.Error
        );

    internal static DiagnosticModel TestDiagnostic(string message) =>
        new DiagnosticModel(
            Code: "Consolo0011",
            Message: message,
            Location: null,
            Severity: DiagnosticSeverity.Warning
        );

    internal static DiagnosticModel DuplicateRootCommand(Option<Location> location) =>
        new DiagnosticModel(
            Code: "Consolo0012",
            Message: "A root command has already been defined.",
            Location: location | null!,
            Severity: DiagnosticSeverity.Error
        );

    internal static DiagnosticModel DuplicateNameOrAlias(
        string name,
        string commandName,
        Option<Location> location) =>
        new DiagnosticModel(
            Code: "Consolo0013",
            Message: $"The parameter name or alias '{name}' is already in use in command '{commandName}'.",
            Location: location | null!,
            Severity: DiagnosticSeverity.Error
        );
}
