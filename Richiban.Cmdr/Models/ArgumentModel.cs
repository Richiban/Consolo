using System;

namespace Richiban.Cmdr.Models
{
    internal class ArgumentModel
    {
        public ArgumentModel(string name, string fullyQualifiedTypeName, bool isFlag)
        {
            Name = name;
            FullyQualifiedTypeName = fullyQualifiedTypeName;
            IsFlag = isFlag;
        }

        public string Name { get; }
        public string FullyQualifiedTypeName { get; }
        public bool IsFlag { get; }
    }
}