namespace Richiban.Cmdr.Models;

internal record ArgumentModel(
    string Name,
    string FullyQualifiedTypeName,
    bool IsFlag,
    bool IsRequired,
    string? DefaultValue,
    string? Description);
