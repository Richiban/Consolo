﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Richiban.CommandLine
{
    class ParameterModelList : IReadOnlyList<ParameterModel>
    {
        private readonly List<ParameterModel> _parameterModels;

        public ParameterModelList(IEnumerable<ParameterInfo> parameters)
        {
            _parameterModels = parameters
                .Select(p => new ParameterModel(p))
                .ToList();

            Help = String.Join(" ", _parameterModels.Select(p => p.Help));
        }

        public ParameterModel this[int index] => _parameterModels[index];

        public string Help { get; }

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

            foreach(var parameter in _parameterModels.Where(p => p.HasShortForm))
            {
                foreach(var shortForm in parameter.ShortForms)
                {
                    if(parts.Contains(shortForm))
                    {
                        parts.Remove(shortForm);
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