using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Cmdr;

internal record CommandMethod(
        string FullyQualifiedClassName,
        string MethodName,
        IReadOnlyCollection<CommandParameter> Parameters,
        Option<string> Description)
{
    public string FullyQualifiedName { get; } = $"{FullyQualifiedClassName}.{MethodName}";

    public ImmutableArray<CommandParameter.Positional> MandatoryParameters { get; } =
        Parameters
            .OfType<CommandParameter.Positional>()
            .ToImmutableArray();

    public ImmutableArray<CommandParameter.OptionalPositional> OptionalParameters { get; } =
        Parameters
            .OfType<CommandParameter.OptionalPositional>()
            .ToImmutableArray();

    public ImmutableArray<CommandParameter.Flag> Flags { get; } =
        Parameters
            .OfType<CommandParameter.Flag>()
            .ToImmutableArray();

    public int MandatoryParameterCount { get; } =
        Parameters
            .OfType<CommandParameter.Positional>()
            .Count();

    public int OptionalParameterCount { get; } =
        Parameters
            .OfType<CommandParameter.OptionalPositional>()
            .Count();

    public int FlagCount =>
        Parameters
            .OfType<CommandParameter.Flag>()
            .Count();
}