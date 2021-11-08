using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.Cmdr.Models
{
    internal class MethodModel
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
        }

        public string FullyQualifiedClassName { get; }
        public string Name { get; }
        public IReadOnlyList<string> ParentNames { get; }
        public IReadOnlyCollection<ArgumentModel> Arguments { get; }
    }
}