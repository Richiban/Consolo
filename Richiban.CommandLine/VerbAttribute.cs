using System;

namespace Richiban.CommandLine
{
    /// <summary>
    /// Use the VerbAttribute to have your command line action respond to a verb.
    /// 
    /// Using the VerbAttribute without argument will infer the verb from the method name.
    /// </summary>
    /// <example>
    /// <code>
    /// [CommandLine, Verb]
    /// public void Execute() // This method will respond to the verb "execute"
    /// {
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// [CommandLine, Verb, Verb("list")]
    /// public void ListItems() // This method will respond to the verbs "listitems" and "list"
    /// {
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// [CommandLine, Verb("delete"), Verb("remove")] // This method will respond to the verbs "delete" and "remove"
    /// public void DeleteItem()
    /// {
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class VerbAttribute : Attribute
    {
        public VerbAttribute()
        {
            Verb = null;
        }

        public VerbAttribute(string verb)
        {
            Verb = verb;
        }

        public string Verb { get; }
    }
}