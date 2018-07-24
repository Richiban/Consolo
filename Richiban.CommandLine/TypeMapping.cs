using System;

namespace Richiban.CommandLine
{
    internal class TypeMapping
    {
        public TypeModel TypeModel { get; }
        public PropertyMappingCollection PropertyMappings { get; }

        public TypeMapping(TypeModel typeModel, PropertyMappingCollection propertyMappings)
        {
            TypeModel = typeModel ?? throw new ArgumentNullException(nameof(typeModel));
            PropertyMappings = propertyMappings ?? throw new ArgumentNullException(nameof(propertyMappings));
        }

        public ICommandLineAction CreateInstance()
        {
            return TypeModel.CreateInstance(PropertyMappings);
        }
    }
}
