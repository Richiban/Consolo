namespace Richiban.Cmdr;

abstract class CommandParameterModel
{
    public abstract string Name { get; }
    public abstract string FullyQualifiedTypeName { get; }
    public abstract Option<string> Description { get; }

    public sealed class CommandPositionalParameterModel(
            string name,
            string fullyQualifiedTypeName,
            Option<string> description) : CommandParameterModel
    {
        public override string Name { get; } = name;
        public override string FullyQualifiedTypeName { get; } = fullyQualifiedTypeName;
        public override Option<string> Description { get; } = description;
    }

    public sealed class CommandOptionalPositionalParameterModel(
            string name,
            string fullyQualifiedTypeName,
            string defaultValue,
            Option<string> description) : CommandParameterModel
    {
        public override string Name { get; } = name;
        public override string FullyQualifiedTypeName { get; } = fullyQualifiedTypeName;
        public override Option<string> Description { get; } = description;
        public string DefaultValue { get; } = defaultValue;
    }

    public sealed class CommandFlagModel(
        string name, Option<string> shortForm, Option<string> description) 
        : CommandParameterModel
    {
        public override string Name { get; } = name;
        public override string FullyQualifiedTypeName { get; } = "System.Boolean";
        public override Option<string> Description { get; } = description;
        public Option<string> ShortForm { get; } = shortForm;
    }
}
