using System;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    internal class XmlComments
    {
        public XmlComments(string methodComments, IReadOnlyDictionary<string, string> parameterComments)
        {
            MethodComments = methodComments;
            ParameterComments = parameterComments;
        }

        public string MethodComments { get; }
        public IReadOnlyDictionary<string, string> ParameterComments { get; }
    }
}