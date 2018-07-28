using System;

namespace Richiban.CommandLine
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class FlagAttribute : System.Attribute
    {
        public FlagAttribute(char shortForm, params string[] names) { }
        public FlagAttribute(string primaryName, params string[] otherNames) { }
    }
}
