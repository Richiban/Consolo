using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class ParameterNameAttribute : Attribute
    {
        public ParameterNameAttribute(params string[] alternativeNames)
        {
            Names = alternativeNames.Distinct().ToList();
        }

        public IReadOnlyList<string> Names { get; }
        public bool IncludeOriginal { get; set;  }
    }
}