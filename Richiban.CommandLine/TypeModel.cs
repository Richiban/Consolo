using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class TypeModel
    {
        private readonly Type _type;

        public TypeModel(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));

            Verbs = type
                .GetCustomAttributes(inherit: true)
                .OfType<VerbAttribute>()
                .SelectMany(v => v.Verbs)
                .ToList();

            CanBeVerbless = Verbs.Count == 0 || Verbs.Contains("");

            Properties =
                _type
                .GetProperties()
                .Where(p => p.CanWrite)
                .Select(prop => new PropertyModel(prop))
                .ToList();

            var propertyHelp = String.Join(" ", Properties.Select(p => p.Help));
            var verbHelp = string.Join("|", Verbs.Except(new[] { "" }));

            if (CanBeVerbless)
                verbHelp = $"[{verbHelp}]";

            Help = $"{verbHelp} {propertyHelp}";
        }

        public IReadOnlyList<PropertyModel> Properties { get; }
        public IReadOnlyList<string> Verbs { get; }
        public bool CanBeVerbless { get; }
        public string Help { get; }

        public ICommandLineAction CreateInstance(IReadOnlyList<(CommandLineArgument, PropertyModel)> arguments)
        {
            var instance = (ICommandLineAction)Activator.CreateInstance(_type);

            foreach (var (arg, prop) in arguments)
            {
                switch (arg)
                {
                    case CommandLineArgument.Free free:
                        prop.SetValue(instance, free.Value);
                        break;
                    case CommandLineArgument.Flag flag:
                        prop.SetValue(instance, true);
                        break;
                    case CommandLineArgument.Named named:
                        prop.SetValue(instance, named.Value);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return instance;
        }

        public bool MatchesVerb(CommandLineArgumentCollection args1, ref CommandLineArgumentCollection args2)
        {
            switch(args1.FirstOrDefault())
            {
                case null when CanBeVerbless:
                    return true;
                case CommandLineArgument.Free free when Verbs.Contains(free.Value, StringComparer.CurrentCultureIgnoreCase):
                    args2 = args1.Without(free);
                    return true;
                default:
                    return false;
            }
        }
    }
}