using System;
using System.ComponentModel;

namespace Richiban.Cmdr;

abstract class CommandParameterModel
{
    private CommandParameterModel(
        string name,
        string fullyQualifiedTypeName,
        bool isRequired,
        Option<string> defaultValue,
        Option<string> description)
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
    public Option<string> DefaultValue { get; }
    public Option<string> Description { get; }

    public sealed class CommandPositionalParameterModel : CommandParameterModel
    {
        public CommandPositionalParameterModel(
            string name,
            string fullyQualifiedTypeName,
            bool isRequired,
            Option<string> defaultValue,
            Option<string> description) : base(name, fullyQualifiedTypeName, isRequired, defaultValue, description)
        {
        }
    }

    public sealed class CommandFlagModel : CommandParameterModel
    {
        public CommandFlagModel(string name, Option<string> shortForm, Option<string> description) 
            : base(name, "System.Boolean", isRequired: false, defaultValue: "false", description)
        {
            ShortForm = shortForm;
        }

        public Option<string> ShortForm { get; }
    }
}
