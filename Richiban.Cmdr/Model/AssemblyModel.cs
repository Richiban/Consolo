﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Richiban.Cmdr
{
    internal class AssemblyModel : IReadOnlyCollection<MethodModel>
    {
        private readonly IReadOnlyCollection<MethodModel> _methodModels;

        public AssemblyModel(IReadOnlyCollection<MethodModel> methodModels)
        {
            _methodModels = methodModels;
        }

        public int Count => _methodModels.Count;
        public IEnumerator<MethodModel> GetEnumerator() => _methodModels.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static AssemblyModel Scan(IEnumerable<Assembly> assembliesToScan)
        {
            var methodModels = (
                from assembly in assembliesToScan
                from type in assembly.GetTypes()
                from method in type.GetMethods()
                where method.GetCustomAttributes(inherit: true)
                    .OfType<CommandLineAttribute>()
                    .Any()
                select BuildMethodModel(method)).ToArray();

            return new AssemblyModel(methodModels);
        }

        private static MethodModel BuildMethodModel(MethodInfo method) => new(method);
    }
}