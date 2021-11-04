using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Richiban.Cmdr.Generator
{
    internal class MethodModel
    {
        public MethodModel(
            string nameIn,
            string className,
            IReadOnlyCollection<ArgumentModel> arguments,
            string[] usings,
            string classNamespace)
        {
            NameIn = nameIn;
            Arguments = arguments;
            ClassName = className;
            NameOut = Utils.ToKebabCase(nameIn);
            Usings = usings.Append(classNamespace).ToImmutableArray();
        }

        public string NameIn { get; }
        public IReadOnlyCollection<ArgumentModel> Arguments { get; }
        public string NameOut { get; }
        public IReadOnlyCollection<string> Usings { get; }
        public string ClassName { get; }
    }

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

    internal class UsingsModel : IEnumerable<string>
    {
        private readonly HashSet<string> _usings = new HashSet<string>();

        public void Add(string @namespace)
        {
            _usings.Add(CleanUpNamespace(@namespace));
        }

        private static string CleanUpNamespace(string @namespace) =>
            @namespace.Replace("using ", "").Replace(";", "");

        public IEnumerator<string> GetEnumerator() => _usings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_usings).GetEnumerator();

        public void AddRange(IEnumerable<string> usings)
        {
            foreach (var @namespace in usings)
            {
                Add(@namespace);
            }
        }
    }
}