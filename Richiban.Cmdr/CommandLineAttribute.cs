using System;

namespace Richiban.Cmdr
{
    /// <summary>
    ///     Tag your methods with this attribute and they will be scanned by Richiban.Cmdr,
    ///     ready to be called.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandLineAttribute : Attribute
    {
    }
}