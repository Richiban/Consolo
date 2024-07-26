using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Consolo;

abstract class ParameterType
{
    private ParameterType() {}

    public abstract string FullyQualifiedTypeName { get; }

    public sealed class Bool() : ParameterType
    {
        public override string FullyQualifiedTypeName { get; } = "bool";
    }

    public sealed class AsIs(string fullyQualifiedTypeName) : ParameterType
    {
        public override string FullyQualifiedTypeName { get; } = fullyQualifiedTypeName;
    }

    public sealed class Constructor(string fullyQualifiedTypeName) : ParameterType
    {
        public override string FullyQualifiedTypeName { get; } = fullyQualifiedTypeName;
    }

    public sealed class ExplicitCast(string fullyQualifiedTypeName) : ParameterType
    {
        public override string FullyQualifiedTypeName { get; } = fullyQualifiedTypeName;
    }

    public sealed class Parse(string fullyQualifiedTypeName, string parseMethodName) : ParameterType
    {
        public override string FullyQualifiedTypeName { get; } = fullyQualifiedTypeName;
        public string ParseMethodName { get; } = parseMethodName;
    }

    public sealed class Enum(string fullyQualifiedTypeName, ImmutableArray<string> enumValues) : ParameterType
    {
        public override string FullyQualifiedTypeName { get; } = fullyQualifiedTypeName;
        public ImmutableArray<string> EnumValues { get; } = enumValues;
    }
}