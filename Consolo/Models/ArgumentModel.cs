using Microsoft.CodeAnalysis;

namespace Consolo;

internal record ParameterModel(
    string Name,
    string SourceName,
    bool IsFlag,
    bool IsRequired,
    Option<string> DefaultValue,
    Option<string> HelpDefaultValue,
    Option<string> Description,
    Option<string> Alias,
    TypeModel Type,
    Option<Location> Location,
    Option<Location> NameLocation,
    Option<Location> AliasLocation,
    ParamsKind Params = ParamsKind.None);

internal enum ParamsKind
{
    None,
    Array,
    // A C# 13 params collection, e.g. params IEnumerable<T> or params List<T>
    Collection
}
