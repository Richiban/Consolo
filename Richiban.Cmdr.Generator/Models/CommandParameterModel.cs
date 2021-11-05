namespace Richiban.Cmdr.Models
{
    public abstract class CommandParameterModel
    {
        private CommandParameterModel(string name)
        {
            Name = name;
        }

        public sealed class CommandPositionalParameterModel : CommandParameterModel
        {
            public CommandPositionalParameterModel(string name) : base(name)
            {
            }
        }

        public string Name { get; }

        public sealed class CommandFlagParameterModel : CommandParameterModel
        {
            public CommandFlagParameterModel(string name) : base(name)
            {
            }
        }
    }
}