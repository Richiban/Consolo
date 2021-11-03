using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.Cmdr
{
    /// <summary>
    /// Define a short form for your flag parameter.
    /// 
    /// Must be used on <see cref="bool"/> parameters only.
    /// </summary>
    public class ShortFormAttribute : Attribute
    {
        /// <summary>
        /// Define one or more short forms for the parameter
        /// </summary>
        /// <param name="firstShortForm">The first short form</param>
        /// <param name="otherShortForms">Any alternative short forms</param>
        public ShortFormAttribute(char firstShortForm, params char[] otherShortForms)
        {
            ShortForms = new[] {firstShortForm }.Concat(otherShortForms).Distinct().ToList();
        }

        /// <summary>
        /// Setting <see cref="DisallowLongForm"/> to true will mean that the parameter can
        /// only be set through its short form
        /// </summary>
        public bool DisallowLongForm { get; set; }

        internal IReadOnlyList<char> ShortForms { get; }
    }
}