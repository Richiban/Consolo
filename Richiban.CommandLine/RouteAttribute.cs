using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    /// <summary>
    /// Use the RouteAttribute to have your command line action respond to a verb. This attribute can be placed 
    /// on a method or a class.
    /// 
    /// Using the RouteAttribute without argument will infer the verb from the method name or class name.
    /// 
    /// When inferring from a class name the suffixes "Action(s)" "CommandLineAction(s)" are stripped off.
    /// </summary>
    /// <example>
    /// <code>
    /// [CommandLine, Route]
    /// public void Execute() // This method will respond to the verb "execute"
    /// {
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// [CommandLine, Route, Route("list")]
    /// public void ListItems() // This method will respond to the verbs "listitems" and "list"
    /// {
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// [CommandLine, Route("delete"), Route("remove")] // This method will respond to the verbs "delete" and "remove"
    /// public void DeleteItem()
    /// {
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RouteAttribute : Attribute
    {
        public RouteAttribute(params string[] routeParts)
        {
            Verbs = routeParts.ToList();
        }

        public IReadOnlyList<string> Verbs { get; }
    }
}