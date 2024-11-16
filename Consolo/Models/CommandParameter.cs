using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Consolo;

abstract class CommandParameter
{
    private CommandParameter() { }
    public abstract Option<string> Name { get; }
    public abstract string SourceName { get; }
    public abstract Option<string> Description { get; }
    public abstract ParameterType Type { get; }
    public string FullyQualifiedTypeName => Type.FullyQualifiedTypeName;
    public abstract string DefaultValue { get; }

    public sealed class Positional(
            string name,
            string sourceName,
            ParameterType type,
            string description) : CommandParameter
    {
        public override Option<string> Name { get; } = name;
        public override ParameterType Type { get; } = type;
        public override Option<string> Description { get; } = description;
        public override string SourceName { get; } = sourceName;
        public override string DefaultValue { get; } = $"default({type.FullyQualifiedTypeName})";
    }

    public sealed class Option(
            Option<string> name,
            ParameterType type,
            string defaultValue,
            Option<string> alias,
            string description,
            string sourceName,
            bool isFlag) : CommandParameter
    {
        public override Option<string> Name { get; } = name;
        public override ParameterType Type { get; } = type;
        public override Option<string> Description { get; } = description;
        public override string SourceName { get; } = sourceName;
        public override string DefaultValue { get; } = defaultValue;
        public Option<string> Alias { get; } = alias;
        public bool IsFlag { get; } = isFlag;
    }
}
