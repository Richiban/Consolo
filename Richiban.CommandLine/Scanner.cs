using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Richiban.CommandLine
{
    internal class Scanner
    {
        private readonly Assembly _assembly;

        public Scanner(Assembly assembly)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        public AssemblyModel BuildModel()
        {
            return
                new AssemblyModel(_assembly
                    .GetTypes()
                    .Where(t => typeof(ICommandLineAction).IsAssignableFrom(t) && t.IsAbstract == false && t.IsClass)
                    .Select(t => new TypeModel(t))
                    .ToArray());
        }
    }
}
