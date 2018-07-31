using System;

namespace Richiban.CommandLine
{
    /// <summary>
    /// Tag your methods with this attribute and they will be scanned by Richiban.CommandLine,
    /// ready to be called.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CommandLineAttribute : Attribute {}
}