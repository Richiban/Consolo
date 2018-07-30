﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    public class ShortFormAttribute : Attribute
    {
        public ShortFormAttribute(params char[] shortForms)
        {
            ShortForms = shortForms.Distinct().ToList();
        }

        public bool DisallowLongForm { get; set; }

        public IReadOnlyList<char> ShortForms { get; }
    }
}