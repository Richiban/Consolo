namespace Richiban.Cmdr;

internal record ParameterModel(
    string Name,
    string FullyQualifiedTypeName,
    bool IsFlag,
    bool IsRequired,
    Option<string> DefaultValue,
    Option<string> Description,
    Option<string> ShortForm);
