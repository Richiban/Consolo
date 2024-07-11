using System;
using System.ComponentModel;

namespace Richiban.Cmdr.Models;

public abstract class CommandParameterModel
{
    private CommandParameterModel(
        string name,
        string fullyQualifiedTypeName,
        bool isRequired,
        string? defaultValue,
        string? description)
    {
        Name = name;
        FullyQualifiedTypeName = fullyQualifiedTypeName;
        IsRequired = isRequired;
        DefaultValue = defaultValue;
        Description = description;
    }

    public string Name { get; }
    public string FullyQualifiedTypeName { get; }
    public bool IsRequired { get; }
    public string? DefaultValue { get; }
    public string? Description { get; }

    public sealed class CommandPositionalParameterModel : CommandParameterModel
    {
        public CommandPositionalParameterModel(
            string name,
            string fullyQualifiedTypeName,
            bool isRequired,
            string? defaultValue,
            string? description) : base(name, fullyQualifiedTypeName, isRequired, defaultValue, description)
        {
        }
    }

    public sealed class CommandFlagModel : CommandParameterModel
    {
        public CommandFlagModel(string name, string? description) 
            : base(name, "System.Boolean", isRequired: false, defaultValue: "false", description)
        {
        }
    }
}
