using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Richiban.CommandLine
{
    class AssemblyModel : IReadOnlyCollection<MethodModel>
    {
        private IReadOnlyCollection<MethodModel> _typeModels;

        public AssemblyModel(IReadOnlyCollection<MethodModel> typeModels)
        {
            _typeModels = typeModels;
        }

        public int Count => _typeModels.Count;
        public IEnumerator<MethodModel> GetEnumerator() => _typeModels.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static AssemblyModel Scan(IEnumerable<Assembly> assembliesToScan)
        {
            CommandLine.Log("Scanning assemblies");

            var methodModels = (
                from assembly in assembliesToScan
                from type in assembly.GetTypes()
                from method in type.GetMethods()
                where method
                        .GetCustomAttributes(inherit: true)
                        .OfType<CommandLineAttribute>()
                        .Any()
                select new MethodModel(method)).ToArray();

            CommandLine.Log("Modelled the following methods:");

            foreach(var method in methodModels)
            {
                CommandLine.Log(method, indentationLevel: 1);
            }

            return new AssemblyModel(methodModels);
        }
    }
}
