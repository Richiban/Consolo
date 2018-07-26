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

        public static AssemblyModel Scan(Assembly assembly)
        {
            return
                new AssemblyModel(
                    assembly
                        .GetTypes()
                        .SelectMany(t => t.GetMethods())
                        .Where(m => m
                            .GetCustomAttributes(inherit: true)
                            .OfType<CommandLineAttribute>()
                            .Any())
                        .Select(m => new MethodModel(m))
                        .ToArray());
        }
    }
}
