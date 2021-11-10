using System;

namespace Richiban.Cmdr
{
    internal class CmdrAttributeDefinition
    {
        public string Namespace => "Richiban.Cmdr";
        public string ShortName => "CmdrMethod";
        public string LongName => $"{ShortName}Attribute";
        public string FullyQualifiedName => $"{Namespace}.{LongName}";
    }
}