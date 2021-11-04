using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Richiban.Cmdr.Generator
{
    internal record MethodModel
    {
        public MethodModel(
            string nameIn,
            string fullyQualifiedClassName,
            IReadOnlyCollection<ArgumentModel> arguments)
        {
            NameIn = nameIn;
            Arguments = arguments;
            FullyQualifiedClassName = fullyQualifiedClassName;
            NameOut = Utils.ToKebabCase(nameIn);
        }

        public string NameIn { get; }
        public IReadOnlyCollection<ArgumentModel> Arguments { get; }
        public string FullyQualifiedClassName { get; }
        public string NameOut { get; }
    }
}