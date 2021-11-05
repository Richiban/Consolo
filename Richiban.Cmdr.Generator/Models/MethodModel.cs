using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.Cmdr.Models
{
    class MethodModel
    {
        public MethodModel(
            string name,
            IReadOnlyList<string> parentNames,
            string fullyQualifiedClassName,
            IReadOnlyCollection<ArgumentModel> arguments)
        {
            Name = name;
            ParentNames = parentNames;
            Arguments = arguments;
            FullyQualifiedClassName = fullyQualifiedClassName;
            NameOut = Utils.ToKebabCase(name);
        }

        public string Name { get; }
        public string NameOut { get; }
        public IReadOnlyList<string> ParentNames { get; }
        public IReadOnlyCollection<ArgumentModel> Arguments { get; }
        public string FullyQualifiedClassName { get; }
    }
}