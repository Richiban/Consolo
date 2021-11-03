using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Richiban.Cmdr
{
    class ParameterModelList : IReadOnlyList<ParameterModel>
    {
        private readonly List<ParameterModel> _parameterModels;

        public ParameterModelList(IEnumerable<ParameterInfo> parameters)
        {
            _parameterModels = parameters
                .Select(p => new ParameterModel(p))
                .ToList();
        }

        public ParameterModel this[int index] => _parameterModels[index];
        
        public int Count => _parameterModels.Count;

        public IEnumerator<ParameterModel> GetEnumerator() => _parameterModels.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public CommandLineArgumentList ExpandShortForms(CommandLineArgumentList arguments)
        {
            foreach(var argument in arguments.OfType<CommandLineArgument.BareNameOrFlag>())
            {
                if(MatchesAllShortForms(argument))
                {
                    return arguments.ExpandShortFormArgument(argument);
                }
            }

            return arguments;
        }

        private bool MatchesAllShortForms(CommandLineArgument.BareNameOrFlag argument)
        {
            var parts = argument.Name.ToCharArray().ToList();

            if (parts.Count == 0)
                return false;

            foreach(var parameter in _parameterModels)
            {
                foreach(var c in parts)
                {
                    if(parameter.MatchesShortForm(c))
                    {
                        parts.Remove(c);
                        break;
                    }
                }
            }

            if (parts.Any())
                return false;

            return true;
        }
    }
}