using System;

namespace Richiban.CommandLine
{
    internal class MethodMapping
    {
        public MethodModel TypeModel { get; }
        public PropertyMappingList PropertyMappings { get; }

        public MethodMapping(MethodModel typeModel, PropertyMappingList propertyMappings)
        {
            TypeModel = typeModel ?? throw new ArgumentNullException(nameof(typeModel));
            PropertyMappings = propertyMappings ?? throw new ArgumentNullException(nameof(propertyMappings));
        }

        public CommandLineAction CreateInstance()
        {
            return TypeModel.CreateAction(PropertyMappings);
        }
    }
}
