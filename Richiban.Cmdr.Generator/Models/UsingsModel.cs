﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Richiban.Cmdr.Models
{
    internal class UsingsModel : IEnumerable<string>
    {
        private readonly HashSet<string> _usings = new HashSet<string>();

        public void Add(string @namespace)
        {
            _usings.Add(CleanUpNamespace(@namespace));
        }

        private static string CleanUpNamespace(string @namespace) =>
            @namespace.Replace("using ", "").Replace(";", "");

        public IEnumerator<string> GetEnumerator() => _usings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_usings).GetEnumerator();

        public void AddRange(IEnumerable<string> usings)
        {
            foreach (var @namespace in usings)
            {
                Add(@namespace);
            }
        }
    }
}