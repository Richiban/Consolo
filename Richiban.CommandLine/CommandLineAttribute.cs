using System;

namespace Richiban.CommandLine
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CommandLineAttribute : Attribute
    {
    }
}