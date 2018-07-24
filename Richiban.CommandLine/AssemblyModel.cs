using System.Collections;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    class AssemblyModel : IReadOnlyCollection<TypeModel>
    {
        private IReadOnlyCollection<TypeModel> _typeModels;

        public AssemblyModel(IReadOnlyCollection<TypeModel> typeModels)
        {
            _typeModels = typeModels;
        }

        public int Count => _typeModels.Count;
        public IEnumerator<TypeModel> GetEnumerator() => _typeModels.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
