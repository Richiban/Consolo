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
        /// <summary>
        /// Define a route
        /// </summary>
        /// <param name="routeParts">The sequence of words that form the route. Empty string is allowed</param>
        public RouteAttribute(params string[] routeParts)
        {
            RouteParts = routeParts.ToList();
        }

        internal IReadOnlyList<string> RouteParts { get; }
    }
}