using System;

namespace Richiban.Cmdr
{
    internal class CmdrAttribute
    {
        public string AttributeName => "CmdrMethod";
        public string Namespace => "Richiban.Cmdr";

        public string FullName => $"{Namespace}.{AttributeName}";
    }
}