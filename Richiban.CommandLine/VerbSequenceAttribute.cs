using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class VerbSequenceAttribute : Attribute
    {
        public VerbSequenceAttribute(string firstVerb, string secondVerb, params string[] subsequentVerbs)
        {
            Verbs = new[] { firstVerb, secondVerb }.Concat(subsequentVerbs).ToList();
        }

        public IReadOnlyList<string> Verbs { get; }
    }
}