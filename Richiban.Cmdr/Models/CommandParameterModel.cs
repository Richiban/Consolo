using System;

namespace Richiban.Cmdr.Models
{
    public abstract class CommandParameterModel
    {
        private CommandParameterModel(string name, string fullyQualifiedTypeName)
        {
            Name = name;
            FullyQualifiedTypeName = fullyQualifiedTypeName;
        }

        public string Name { get; }
        public string FullyQualifiedTypeName { get; }

        public sealed class CommandPositionalParameterModel : CommandParameterModel
        {
            public CommandPositionalParameterModel(
                string name,
                string fullyQualifiedTypeName) : base(name, fullyQualifiedTypeName)
            {
            }
        }

        public sealed class CommandFlagModel : CommandParameterModel
        {
            public CommandFlagModel(string name) : base(name, "System.Boolean")
            {
            }
        }
    }
}