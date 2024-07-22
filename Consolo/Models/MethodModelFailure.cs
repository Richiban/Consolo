using System;
using Microsoft.CodeAnalysis;

namespace Consolo;

record DiagnosticModel(string Code, string Message, Location? Location, DiagnosticSeverity Severity)
{
    public static DiagnosticModel NoMethodsFound() =>
        new DiagnosticModel(
            Code: "Consolo0002",
            Message: $"No command methods found. Make sure you're using the {ConsoloAttributeDefinition.ShortName} attribute",
            Location: null,
            DiagnosticSeverity.Error);

    public static DiagnosticModel CommandNameAlreadyInUse(string commandName, Location location) =>
        new DiagnosticModel(
            Code: "Consolo0003",
            Message: $"The command name '{commandName}' is already in use.",
            Location: location,
            DiagnosticSeverity.Error);

    internal static DiagnosticModel ErrorProcessingMethod(
        string message,
        Location? Location)
    {
        return new DiagnosticModel(
            Code: "Consolo0004",
            Message: message,
            Location: Location,
            Severity: DiagnosticSeverity.Error
        );
    }

    internal static DiagnosticModel MethodMustBeStatic(
        IMethodSymbol methodSymbol,
        Location? location)
    {
        return new DiagnosticModel(
            Code: "Consolo0005",
            Message: $"Method {methodSymbol} must be static in order to use the {ConsoloAttributeDefinition.ShortName} attribute.",
            Location: location,
            Severity: DiagnosticSeverity.Error
        );
    }

    internal static DiagnosticModel AttributeProblem(
        string errTypeName,
        CandidateReason candidateReason,
        Location? Location)
    {
        return new DiagnosticModel(
            Code: "Consolo0006",
            Message: $"There was a problem with attribute {errTypeName}: {candidateReason}",
            Location: Location,
            Severity: DiagnosticSeverity.Error
        );
    }

    internal static DiagnosticModel MultipleParameterNamesSupplied(
        IParameterSymbol parameterSymbol,
        int suppliedNameCount,
        Location? Location)
    {
        return new DiagnosticModel(
            Code: "Consolo0007",
            Message: $"Parameter {parameterSymbol.Name} has {suppliedNameCount} names specified in the attribute; parameters can only specify one.",
            Location: Location,
            Severity: DiagnosticSeverity.Error
        );
    }

    internal static DiagnosticModel UnsupportedParameterType(ParameterModel param)
    {
        return new DiagnosticModel(
            Code: "Consolo0008",
            Message: $"Parameter '{param.Name}' has a type that is unsupported ({param.Type.Name}).",
            Location: param.Location,
            Severity: DiagnosticSeverity.Error
        );
    }
}
