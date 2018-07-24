using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Richiban.CommandLine
{
    internal class PropertyModel
    {
        private readonly PropertyInfo _propertyInfo;
        private readonly IReadOnlyCollection<string> _names;

        public PropertyModel(PropertyInfo prop)
        {
            _propertyInfo = prop;

            IsOptional = prop.CustomAttributes
                .Any(a => typeof(OptionalAttribute).IsAssignableFrom(a.AttributeType));

            Name = prop.Name;

            _names =
                (new[] { prop.Name })
                .Concat(prop
                    .GetCustomAttributes(inherit: true)
                    .OfType<AlternativeNameAttribute>()
                    .SelectMany(attr => attr.AlternativeNames))
                .ToList();

            PropertyType = prop.PropertyType;

            var helpForm = $"<{Name.ToLowerInvariant()}>";

            Help = IsOptional ? $"[{helpForm}]" : helpForm;
        }

        public string Name { get; }
        public Type PropertyType { get; }
        public string Help { get; }

        public bool IsOptional { get; }
        public string DefaultValue { get; }

        public bool NameMatches(string argumentName)
        {
            return _names.Any(n => n.StartsWith(argumentName, StringComparison.CurrentCultureIgnoreCase));
        }

        internal void SetValue(ICommandLineAction instance, object value) =>
            _propertyInfo.SetValue(instance, value);
    }
}