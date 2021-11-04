using System;

namespace Richiban.Cmdr.Generator
{
    internal class ArgumentModel
    {
        public ArgumentModel(string nameIn, string fullyQualifiedTypeName, bool isFlag)
        {
            NameIn = nameIn;
            FullyQualifiedTypeName = fullyQualifiedTypeName;
            IsFlag = isFlag;
            NameOut = Utils.ToKebabCase(nameIn);
        }

        public string NameIn { get; }
        public string NameOut { get; }
        public string FullyQualifiedTypeName { get; }
        public bool IsFlag { get; }
    }
}