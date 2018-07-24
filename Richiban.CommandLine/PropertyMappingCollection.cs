using System.Collections;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    internal class PropertyMappingCollection : IReadOnlyList<(CommandLineArgument, PropertyModel)>
    {
        private List<(CommandLineArgument, PropertyModel)> _namedPairings;

        public PropertyMappingCollection(List<(CommandLineArgument, PropertyModel)> namedPairings)
        {
            _namedPairings = namedPairings;
        }

        public (CommandLineArgument, PropertyModel) this[int index] =>
            _namedPairings[index];

        public int Count => _namedPairings.Count;

        public IEnumerator<(CommandLineArgument, PropertyModel)> GetEnumerator() =>
            _namedPairings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}