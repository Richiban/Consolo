namespace Richiban.Cmdr;

abstract class CommandParameter
{
    private CommandParameter() { }
    public abstract string Name { get; }
    public abstract string FullyQualifiedTypeName { get; }
    public abstract Option<string> Description { get; }

    public sealed class Positional(
            string name,
            string fullyQualifiedTypeName,
            Option<string> description) : CommandParameter
    {
        public override string Name { get; } = name;
        public override string FullyQualifiedTypeName { get; } = fullyQualifiedTypeName;
        public override Option<string> Description { get; } = description;
    }

    public sealed class OptionalPositional(
            string name,
            string fullyQualifiedTypeName,
            string defaultValue,
            Option<string> description) : CommandParameter
    {
        public override string Name { get; } = name;
        public override string FullyQualifiedTypeName { get; } = fullyQualifiedTypeName;
        public override Option<string> Description { get; } = description;
        public string DefaultValue { get; } = defaultValue;
    }

    public sealed class Flag(
        string name, Option<string> shortForm, Option<string> description) 
        : CommandParameter
    {
        public override string Name { get; } = name;
        public override string FullyQualifiedTypeName { get; } = "System.Boolean";
        public override Option<string> Description { get; } = description;
        public Option<string> ShortForm { get; } = shortForm;
    }
}
