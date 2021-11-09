using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.Cmdr.Models
{
    internal class MethodModel
    {
        public MethodModel(
            string methodName,
            string? providedName,
            IReadOnlyList<string> groupCommandPath,
            string fullyQualifiedClassName,
            IReadOnlyCollection<ArgumentModel> arguments)
        {
            MethodName = methodName;
            ProvidedName = providedName;
            GroupCommandPath = groupCommandPath;
            Arguments = arguments;
            FullyQualifiedClassName = fullyQualifiedClassName;
        }

        public string FullyQualifiedClassName { get; }
        public string MethodName { get; }
        public string? ProvidedName { get; }
        public IReadOnlyList<string> GroupCommandPath { get; }
        public IReadOnlyCollection<ArgumentModel> Arguments { get; }
    }
}