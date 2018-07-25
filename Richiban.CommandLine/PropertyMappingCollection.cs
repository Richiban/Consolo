using System.Collections;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    internal class PropertyMappingList : IReadOnlyList<PropertyMapping>
    {
        private List<PropertyMapping> _namedPairings;

        public PropertyMappingList(List<PropertyMapping> namedPairings)
        {
            _namedPairings = namedPairings;
        }

        public PropertyMapping this[int index] =>
            _namedPairings[index];

        public int Count => _namedPairings.Count;

        public IEnumerator<PropertyMapping> GetEnumerator() =>
            _namedPairings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}