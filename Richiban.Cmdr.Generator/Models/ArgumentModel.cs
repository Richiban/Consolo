using System;

namespace Richiban.Cmdr.Generator
{
    internal class ArgumentModel
    {
        public ArgumentModel(string nameIn, string type, bool isFlag)
        {
            NameIn = nameIn;
            Type = type;
            IsFlag = isFlag;
            NameOut = Utils.ToKebabCase(nameIn);
        }

        public string NameIn { get; }
        public string NameOut { get; }
        public string Type { get; }
        public bool IsFlag { get; }
    }
}