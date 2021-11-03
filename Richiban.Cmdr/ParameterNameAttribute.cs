using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.Cmdr
{
    /// <summary>
    /// Define alternative names for a parameter. Primarily used if you want your parameter to have
    /// a name that is not a legal C# identifier, for example: "my-param-1".
    /// 
    /// If you want to define <i>additional</i> names then set IncludeOriginal to true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class ParameterNameAttribute : Attribute
    {
        /// <summary>
        /// Define the alternative names for a parameter
        /// </summary>
        /// <param name="alternativeNames">The alternative names for the given parameter</param>
        public ParameterNameAttribute(params string[] alternativeNames)
        {
            Names = alternativeNames.Distinct().ToList();
        }

        internal IReadOnlyList<string> Names { get; }

        /// <summary>
        /// Setting <see cref="IncludeOriginal"/> to true will use the given alternative names 
        /// <strong>in addition</strong> to the original name
        /// </summary>
        public bool IncludeOriginal { get; set;  }
    }
}