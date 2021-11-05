using System;

namespace Richiban.Cmdr
{
    class CmdrAttribute
    {
        public string AttributeName => "CmdrMethod";
        public string Namespace => "Richiban.Cmdr";

        public string FullName => $"{Namespace}.{AttributeName}";
    }
}